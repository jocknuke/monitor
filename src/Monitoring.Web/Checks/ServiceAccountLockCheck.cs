
using Dapper;
using Microsoft.Data.SqlClient;
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Checks;

[Check("serviceaccount-locks")]
public class ServiceAccountLockCheck : ICheck
{
    private const string Sql = @"
SELECT name, is_disabled, LOGINPROPERTY(name, 'LockoutTime') AS LockoutTime
FROM sys.sql_logins WHERE name = @login";

    public async Task<CheckResult> RunAsync(CheckDescriptor d, CancellationToken ct)
    {
        var cs = d.Parameters.GetValueOrDefault("connectionString") ?? "";
        var login = d.Parameters.GetValueOrDefault("login") ?? "";
        if (string.IsNullOrWhiteSpace(cs) || string.IsNullOrWhiteSpace(login))
            return new CheckResult(d.Id, DateTimeOffset.UtcNow, CheckStatus.Unknown, "connectionString or login missing");

        try
        {
            await using var conn = new Microsoft.Data.SqlClient.SqlConnection(cs);
            await conn.OpenAsync(ct);
            var row = await conn.QueryFirstOrDefaultAsync(Sql, new { login });
            if (row is null) return new CheckResult(d.Id, DateTimeOffset.UtcNow, CheckStatus.Unknown, $"Login not found: {login}");

            bool disabled = (row.is_disabled ?? false);
            var status = disabled ? CheckStatus.Unhealthy : CheckStatus.Healthy;
            var msg = disabled ? "Login disabled" : "OK";
            var dims = new Dictionary<string,string>{{"login", login}};
            return new CheckResult(d.Id, DateTimeOffset.UtcNow, status, msg, null, dims);
        }
        catch (Exception ex)
        {
            return new CheckResult(d.Id, DateTimeOffset.UtcNow, CheckStatus.Unhealthy, $"Exception: {ex.Message}");
        }
    }
}
