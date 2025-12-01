using Monitoring.Web.Contracts;
using Oracle.ManagedDataAccess.Client;

namespace Monitoring.Web.Checks;

[Check("oracle-balance")]
public class OracleBalanceCheck : ICheck
{
    public async Task<CheckResult> RunAsync(CheckDescriptor d, CancellationToken ct)
    {
        var srcConn = d.Parameters["sourceConnection"];
        var tgtConn = d.Parameters["targetConnection"];
        var srcQuery = d.Parameters["sourceQuery"];
        var tgtQuery = d.Parameters["targetQuery"];
        var tolerancePct = double.TryParse(d.Parameters.GetValueOrDefault("tolerancePct"), out var t) ? t : 0.0;

        double srcVal = await GetSumAsync(srcConn, srcQuery, ct);
        double tgtVal = await GetSumAsync(tgtConn, tgtQuery, ct);

        var diff = tgtVal - srcVal;
        var pctDiff = srcVal != 0 ? Math.Abs(diff / srcVal * 100.0) : 0.0;
        var status = pctDiff <= tolerancePct ? CheckStatus.Healthy : CheckStatus.Unhealthy;

        var msg = $"Source={srcVal:N2}, Target={tgtVal:N2}, Diff={diff:N2} ({pctDiff:N2}%)";

        return new CheckResult(
            d.Id,
            DateTimeOffset.UtcNow,
            status,
            msg,
            new Dictionary<string, double>
            {
                ["source_sum"] = srcVal,
                ["target_sum"] = tgtVal,
                ["diff"] = diff,
                ["pct_diff"] = pctDiff
            });
    }

    private async Task<double> GetSumAsync(string connStr, string query, CancellationToken ct)
    {
        await using var conn = new OracleConnection(connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new OracleCommand(query, conn);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result == DBNull.Value ? 0 : Convert.ToDouble(result);
    }
}
