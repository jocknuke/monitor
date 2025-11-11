
using Cronos;
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Services;

public class CheckSchedulerService : BackgroundService
{
    private readonly ICheckStore _store;
    private readonly IResultStore _results;
    private readonly ICheckRegistry _registry;
    private readonly ILogger<CheckSchedulerService> _log;

    private readonly Dictionary<string, DateTimeOffset> _nextRun = new();
    private IReadOnlyList<CheckDescriptor> _lastSnapshot = Array.Empty<CheckDescriptor>();

    public CheckSchedulerService(ICheckStore store, IResultStore results, ICheckRegistry registry, ILogger<CheckSchedulerService> log)
        => (_store, _results, _registry, _log) = (store, results, registry, log);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var checks = await _store.ListAsync(stoppingToken);

            // Hot-reload: if descriptors changed, reset nextRun
            if (!ReferenceEquals(checks, _lastSnapshot) && checks.Count != _lastSnapshot.Count)
                _nextRun.Clear();
            _lastSnapshot = checks;

            var now = DateTimeOffset.UtcNow;
            foreach (var c in checks)
            {
                if (!c.Enabled) continue;
                if (!_nextRun.TryGetValue(c.Id, out var due)) _nextRun[c.Id] = now;

                if (now >= _nextRun[c.Id])
                {
                    _ = RunOne(c, stoppingToken);
                    _nextRun[c.Id] = ComputeNextRun(now, c);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private DateTimeOffset ComputeNextRun(DateTimeOffset now, CheckDescriptor c)
    {
        if (!string.IsNullOrWhiteSpace(c.Cron))
        {
            var expr = CronExpression.Parse(c.Cron);
            var next = expr.GetNextOccurrence(now.UtcDateTime, TimeZoneInfo.Utc);
            return next.HasValue ? new DateTimeOffset(next.Value, TimeSpan.Zero) : now.AddMinutes(5);
        }
        var secs = c.IntervalSeconds.GetValueOrDefault(60);
        if (secs < 5) secs = 5;
        return now.AddSeconds(secs);
    }

    private async Task RunOne(CheckDescriptor d, CancellationToken ct)
    {
        var impl = _registry.Resolve(d.Type);
        if (impl is null) return;
        try
        {
            var r = await impl.RunAsync(d, ct);
            await _results.AppendAsync(r, ct);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Check {Id} threw", d.Id);
            var r = new CheckResult(d.Id, DateTimeOffset.UtcNow, CheckStatus.Unhealthy, $"Exception: {ex.Message}");
            await _results.AppendAsync(r, ct);
        }
    }
}
