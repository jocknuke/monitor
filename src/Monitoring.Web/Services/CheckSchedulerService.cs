
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Services;

public class CheckSchedulerService : BackgroundService
{
    private readonly ICheckStore _store;
    private readonly IResultStore _results;
    private readonly ICheckRegistry _registry;
    private readonly Dictionary<string, DateTimeOffset> _nextRun = new();

    public CheckSchedulerService(ICheckStore store, IResultStore results, ICheckRegistry registry)
        => (_store, _results, _registry) = (store, results, registry);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var checks = await _store.ListAsync(stoppingToken);
            var now = DateTimeOffset.UtcNow;
            foreach (var c in checks)
            {
                if (!c.Enabled) continue;
                if (!_nextRun.TryGetValue(c.Id, out var due)) _nextRun[c.Id] = now;
                if (now >= _nextRun[c.Id])
                {
                    _ = RunOne(c, stoppingToken);
                    _nextRun[c.Id] = now.Add(c.Interval);
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
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
            var r = new CheckResult(d.Id, DateTimeOffset.UtcNow, CheckStatus.Unhealthy, $"Exception: {ex.Message}");
            await _results.AppendAsync(r, ct);
        }
    }
}
