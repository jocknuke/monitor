using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Checks
{
    /// <summary>
    /// Executes an HTTP GET request against a configured endpoint and reports
    /// latency and status. The check reads the "url" parameter from the
    /// descriptor's Parameters dictionary. A non-2xx status code produces
    /// an Unhealthy result.
    /// </summary>
    [Check("http")]
    public class HttpEndpointCheck : ICheck
    {
        private readonly HttpClient _http;

        public HttpEndpointCheck(HttpClient http)
        {
            _http = http;
        }

        public async Task<CheckResult> RunAsync(CheckDescriptor descriptor, CancellationToken ct)
        {
            if (!descriptor.Parameters.TryGetValue("url", out var url) || string.IsNullOrWhiteSpace(url))
            {
                return new CheckResult(
                    descriptor.Id,
                    DateTimeOffset.UtcNow,
                    CheckStatus.Unhealthy,
                    "No URL provided in parameters");
            }
            var dims = new Dictionary<string, string> { ["url"] = url };
            var sw = Stopwatch.StartNew();
            try
            {
                using var resp = await _http.GetAsync(url, ct);
                sw.Stop();
                var status = resp.IsSuccessStatusCode ? CheckStatus.Healthy : CheckStatus.Unhealthy;
                var metrics = new Dictionary<string, double> { ["latency_ms"] = sw.Elapsed.TotalMilliseconds };
                var message = resp.StatusCode.ToString();
                return new CheckResult(
                    descriptor.Id,
                    DateTimeOffset.UtcNow,
                    status,
                    message,
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