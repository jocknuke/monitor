using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monitoring.Web.Contracts;
using Microsoft.AspNetCore.SignalR;
using Cronos;

namespace Monitoring.Web.Services
{
    /// <summary>
    /// Background service that schedules and executes checks at their configured
    /// intervals. For each check descriptor, the scheduler tracks the next run time
    /// and executes checks when due. Results are stored in the result store and
    /// broadcast over SignalR.
    /// </summary>
    public class Scheduler : BackgroundService
    {
        private readonly ICheckStore _checkStore;
        private readonly IResultStore _resultStore;
        private readonly ICheckRegistry _registry;
        private readonly IHubContext<ResultsHub> _hub;
        private readonly ILogger<Scheduler>? _logger;

        // Tracks next scheduled run per check
        private readonly Dictionary<string, DateTimeOffset> _nextRuns = new();
        // Cache parsed cron expressions per check Id
        private readonly Dictionary<string, CronExpression> _cronExpressions = new();

        public Scheduler(
            ICheckStore checkStore,
            IResultStore resultStore,
            ICheckRegistry registry,
            IHubContext<ResultsHub> hub,
            ILogger<Scheduler>? logger = null)
        {
            _checkStore = checkStore;
            _resultStore = resultStore;
            _registry = registry;
            _hub = hub;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var descriptors = await _checkStore.ListAsync();
                var now = DateTimeOffset.UtcNow;
                foreach (var desc in descriptors)
                {
                    if (!desc.Enabled) continue;
                    // Determine whether the check should run based on cron expression or interval.
                    if (desc.Parameters != null && desc.Parameters.TryGetValue("cron", out var cronExpr) && !string.IsNullOrWhiteSpace(cronExpr))
                    {
                        // Cron-based scheduling
                        if (!_cronExpressions.TryGetValue(desc.Id, out var cron))
                        {
                            try
                            {
                                cron = CronExpression.Parse(cronExpr);
                                _cronExpressions[desc.Id] = cron;
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError(ex, "Invalid cron expression '{Cron}' for check {Name}", cronExpr, desc.Name);
                                continue;
                            }
                        }
                        // Initialize the next run if not scheduled yet
                        if (!_nextRuns.TryGetValue(desc.Id, out var nextRun))
                        {
                            var occurrence = cron.GetNextOccurrence(now, TimeZoneInfo.Utc);
                            _nextRuns[desc.Id] = occurrence ?? now.AddMinutes(5);
                        }
                        // Check if it's time to run
                        if (_nextRuns.TryGetValue(desc.Id, out var scheduled) && now >= scheduled)
                        {
                            // Schedule run and compute subsequent next run
                            _ = Task.Run(async () => await RunCheckAsync(desc, stoppingToken), stoppingToken);
                            var nextOcc = cron.GetNextOccurrence(now.AddSeconds(1), TimeZoneInfo.Utc);
                            _nextRuns[desc.Id] = nextOcc ?? now.AddMinutes(5);
                        }
                    }
                    else
                    {
                        // Interval-based scheduling
                        if (!_nextRuns.TryGetValue(desc.Id, out var nextRun))
                        {
                            _nextRuns[desc.Id] = now;
                            nextRun = now;
                        }
                        if (_nextRuns.TryGetValue(desc.Id, out var scheduled) && now >= scheduled)
                        {
                            _ = Task.Run(async () => await RunCheckAsync(desc, stoppingToken), stoppingToken);
                            _nextRuns[desc.Id] = now + desc.Interval;
                        }
                    }
                }
                // Poll every second for due checks
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // ignore cancellation
                }
            }
        }

        private async Task RunCheckAsync(CheckDescriptor desc, CancellationToken ct)
        {
            try
            {
                var impl = _registry.GetCheck(desc.Type);
                if (impl == null)
                {
                    _logger?.LogWarning("No ICheck implementation registered for type {Type}", desc.Type);
                    return;
                }
                var result = await impl.RunAsync(desc, ct);
                await _resultStore.AddResultAsync(result);
                // broadcast result to clients via SignalR
                await _hub.Clients.All.SendAsync("result", result, cancellationToken: ct);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error running check {Check}", desc.Name);
            }
        }
    }
}