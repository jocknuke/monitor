
using Microsoft.Data.SqlClient;
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Checks;

[Check("db-connection")]
public class DbConnectionCheck : ICheck
{
    public async Task<CheckResult> RunAsync(CheckDescriptor d, CancellationToken ct)
    {
        var cs = d.Parameters.TryGetValue("connectionString", out var v) ? v : null;
        var test = d.Parameters.TryGetValue("testQuery", out var q) ? q : null;
        if (string.IsNullOrWhiteSpace(cs))
            return new CheckResult(d.Id, DateTimeOffset.UtcNow, CheckStatus.Unknown, "connectionString missing");

        try
        {
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync(ct);
            if (!string.IsNullOrWhiteSpace(test))
            {
                await using var cmd = new SqlCommand(test, conn);
                var o = await cmd.ExecuteScalarAsync(ct);
            }
            return new CheckResult(d.Id, DateTimeOffset.UtcNow, CheckStatus.Healthy, "Connected");
        }
        catch (Exception ex)
        {
            return new CheckResult(d.Id, DateTimeOffset.UtcNow, CheckStatus.Unhealthy, $"Exception: {ex.Message}");
        }
    }
}
