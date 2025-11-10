using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Checks
{
    /// <summary>
    /// Attempts to open and close a database connection. The check reads
    /// a connection string from the "connectionString" parameter and an
    /// optional provider invariant name from the "provider" parameter
    /// (e.g. "System.Data.SqlClient" or "Npgsql"). When the provider is
    /// omitted, the check attempts to infer a provider from the connection
    /// string. For simplicity, this sample uses DbProviderFactories which
    /// must be configured in your app.config or by referencing the
    /// appropriate ADO.NET provider package. An exception during
    /// connection produces an Unhealthy result.
    /// </summary>
    [Check("db")]
    public class DbConnectionCheck : ICheck
    {
        public async Task<CheckResult> RunAsync(CheckDescriptor descriptor, CancellationToken ct)
        {
            if (!descriptor.Parameters.TryGetValue("connectionString", out var connStr) || string.IsNullOrWhiteSpace(connStr))
            {
                return new CheckResult(
                    descriptor.Id,
                    DateTimeOffset.UtcNow,
                    CheckStatus.Unhealthy,
                    "No connectionString provided");
            }
            descriptor.Parameters.TryGetValue("provider", out var providerName);

            var dims = new Dictionary<string, string> { ["provider"] = providerName ?? "unknown" };
            var sw = Stopwatch.StartNew();
            try
            {
                DbProviderFactory factory;
                if (!string.IsNullOrEmpty(providerName))
                {
                    factory = DbProviderFactories.GetFactory(providerName);
                }
                else
                {
                    // Fallback to SqlClient if provider not specified. Requires Microsoft.Data.SqlClient package.
                    factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
                }
                using var conn = factory.CreateConnection();
                if (conn == null)
                {
                    throw new InvalidOperationException($"Unable to create connection for provider {providerName}");
                }
                conn.ConnectionString = connStr;
                await conn.OpenAsync(ct);
                await conn.CloseAsync();
                sw.Stop();
                var metrics = new Dictionary<string, double> { ["latency_ms"] = sw.Elapsed.TotalMilliseconds };
                return new CheckResult(
                    descriptor.Id,
                    DateTimeOffset.UtcNow,
                    CheckStatus.Healthy,
                    "DB connection OK",
                    metrics,
                    dims);
            }
            catch (Exception ex)
            {
                sw.Stop();
                var metrics = new Dictionary<string, double> { ["latency_ms"] = sw.Elapsed.TotalMilliseconds };
                return new CheckResult(
                    descriptor.Id,
                    DateTimeOffset.UtcNow,
                    CheckStatus.Unhealthy,
                    ex.Message,
                    metrics,
                    dims);
            }
        }
    }
}