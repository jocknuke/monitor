using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Checks
{
    /// <summary>
    /// A simple demo check that always returns a healthy status and a dummy latency metric.
    /// Use this check to verify that the scheduler and result pipeline are functioning.
    /// </summary>
    [Check("ping")]
    public class SamplePingCheck : ICheck
    {
        public Task<CheckResult> RunAsync(CheckDescriptor descriptor, CancellationToken ct)
        {
            var metrics = new Dictionary<string, double> { ["latency_ms"] = 10 };
            var dims = new Dictionary<string, string> { ["name"] = descriptor.Name };
            var result = new CheckResult(
                descriptor.Id,
                DateTimeOffset.UtcNow,
                CheckStatus.Healthy,
                "Ping OK",
                metrics,
                dims);
            return Task.FromResult(result);
        }
    }
}