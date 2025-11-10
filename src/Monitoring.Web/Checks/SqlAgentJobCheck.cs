
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Checks;

[Check("sql-agent-job")]
public class SqlAgentJobCheck : ICheck
{
    private readonly Monitoring.Web.Services.SqlJobService _svc;
    public SqlAgentJobCheck(Monitoring.Web.Services.SqlJobService svc) => _svc = svc;

    public async Task<CheckResult> RunAsync(CheckDescriptor d, CancellationToken ct)
    {
        var cs = d.Parameters.TryGetValue("connectionString", out var v) ? v : null;
        var job = d.Parameters.TryGetValue("jobName", out var j) ? j : null;
        var takeStr = d.Parameters.TryGetValue("historyCount", out var t) ? t : "30";
        int.TryParse(takeStr, out var take); if (take <= 0) take = 30;

        if (string.IsNullOrWhiteSpace(cs) || string.IsNullOrWhiteSpace(job))
            return new CheckResult(d.Id, DateTimeOffset.UtcNow, CheckStatus.Unknown, "connectionString or jobName missing");

        var data = await _svc.GetJobSummaryAndHistoryAsync(cs!, job!, take, ct);
        if (!data.exists)
            return new CheckResult(d.Id, DateTimeOffset.UtcNow, CheckStatus.Unknown, $"Job not found: {job}");

        var status = data.lastStatus switch
        {
            1 => CheckStatus.Healthy,
            0 => CheckStatus.Unhealthy,
            3 or null => CheckStatus.Unknown,
            _ => CheckStatus.Degraded
        };

        var metrics = new Dictionary<string,double>();
        if (data.history.Count > 0)
        {
            metrics["last_duration_sec"] = data.history.Last().durationSec;
            metrics["avg_duration_sec"] = data.history.Average(h => (double)h.durationSec);
        }

        var dims = new Dictionary<string,string>{
            { "job", job! },
            { "last_run_at", data.lastRunAt?.ToString("o") ?? "" },
            { "history_count", data.history.Count.ToString() }
        };

        return new CheckResult(d.Id, DateTimeOffset.UtcNow, status, $"Job '{job}' last status={data.lastStatus}", metrics, dims);
    }
}
