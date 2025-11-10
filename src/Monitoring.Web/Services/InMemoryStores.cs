using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Services
{
    /// <summary>
    /// Interface for a repository of check descriptors. The UI and scheduler use
    /// this abstraction to create, update and list checks.
    /// </summary>
    public interface ICheckStore
    {
        Task<IEnumerable<CheckDescriptor>> ListAsync();
        Task UpsertAsync(CheckDescriptor descriptor);
    }

    /// <summary>
    /// Interface for storing check results. This repository maintains a limited
    /// buffer of recent results per check. The UI consumes these results to
    /// display history or trends.
    /// </summary>
    public interface IResultStore
    {
        Task AddResultAsync(CheckResult result);
        Task<IEnumerable<CheckResult>> ListAsync(string checkId, int max = 100);
    }

    /// <summary>
    /// Simple in-memory implementation of ICheckStore. This implementation is
    /// suitable for demonstration and testing; production systems should replace
    /// this with a durable store such as a database.
    /// </summary>
    public class InMemoryCheckStore : ICheckStore
    {
        private readonly List<CheckDescriptor> _descriptors = new();

        public Task<IEnumerable<CheckDescriptor>> ListAsync()
        {
            return Task.FromResult(_descriptors.AsEnumerable());
        }

        public Task UpsertAsync(CheckDescriptor descriptor)
        {
            var index = _descriptors.FindIndex(d => d.Id == descriptor.Id);
            if (index >= 0)
            {
                _descriptors[index] = descriptor;
            }
            else
            {
                _descriptors.Add(descriptor);
            }
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// In-memory result store that keeps a sliding window of check results in a
    /// thread-safe dictionary keyed by checkId. The window size can be tuned by
    /// adjusting the maxItems parameter when enumerating results.
    /// </summary>
    public class InMemoryResultStore : IResultStore
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<CheckResult>> _results = new();
        private readonly int _maxItems;

        public InMemoryResultStore(int maxItems = 200)
        {
            _maxItems = maxItems;
        }

        public Task AddResultAsync(CheckResult result)
        {
            var queue = _results.GetOrAdd(result.CheckId, _ => new ConcurrentQueue<CheckResult>());
            queue.Enqueue(result);
            while (queue.Count > _maxItems && queue.TryDequeue(out _))
            {
                // discard old results
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<CheckResult>> ListAsync(string checkId, int max = 100)
        {
            if (_results.TryGetValue(checkId, out var queue))
            {
                // return most recent 'max' results in reverse chronological order
                var list = queue.Reverse().Take(max);
                return Task.FromResult(list);
            }
            return Task.FromResult(Enumerable.Empty<CheckResult>());
        }
    }
}