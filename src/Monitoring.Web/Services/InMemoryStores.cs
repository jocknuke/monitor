
using System.Collections.Concurrent;
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Services;

public interface ICheckStore
{
    Task<IReadOnlyList<CheckDescriptor>> ListAsync(CancellationToken ct = default);
    Task UpsertAsync(CheckDescriptor d, CancellationToken ct = default);
    Task<CheckDescriptor?> GetAsync(string id, CancellationToken ct = default);
}

public class InMemoryCheckStore : ICheckStore
{
    private readonly ConcurrentDictionary<string, CheckDescriptor> _checks = new();
    public Task<IReadOnlyList<CheckDescriptor>> ListAsync(CancellationToken ct = default)
        => Task.FromResult((IReadOnlyList<CheckDescriptor>)_checks.Values.OrderBy(c => c.Name).ToList());
    public Task UpsertAsync(CheckDescriptor d, CancellationToken ct = default)
    { _checks[d.Id] = d; return Task.CompletedTask; }
    public Task<CheckDescriptor?> GetAsync(string id, CancellationToken ct = default)
        => Task.FromResult(_checks.TryGetValue(id, out var d) ? d : null);
}

public interface IResultStore
{
    Task AppendAsync(CheckResult r, CancellationToken ct = default);
    Task<CheckResult?> GetLatestAsync(string checkId, CancellationToken ct = default);
    Task<IReadOnlyList<CheckResult>> ListRecentAsync(string checkId, int take = 30, CancellationToken ct = default);
}

public class InMemoryResultStore : IResultStore
{
    private readonly LinkedList<CheckResult> _recent = new();
    private readonly object _lock = new();

    public Task AppendAsync(CheckResult r, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _recent.AddFirst(r);
            while (_recent.Count > 2000) _recent.RemoveLast();
        }
        return Task.CompletedTask;
    }

    public Task<CheckResult?> GetLatestAsync(string checkId, CancellationToken ct = default)
    {
        lock (_lock) return Task.FromResult(_recent.FirstOrDefault(x => x.CheckId == checkId));
    }

    public Task<IReadOnlyList<CheckResult>> ListRecentAsync(string checkId, int take = 30, CancellationToken ct = default)
    {
        lock (_lock) return Task.FromResult((IReadOnlyList<CheckResult>)_recent.Where(x => x.CheckId == checkId).Take(take).Reverse().ToList());
    }
}
