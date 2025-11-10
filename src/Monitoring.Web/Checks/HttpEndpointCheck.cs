
using System.Diagnostics;
using System.Net.Http;
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Checks;

[Check("http")]
public class HttpEndpointCheck : ICheck
{
    private readonly HttpClient _http;
    public HttpEndpointCheck(HttpClient http) => _http = http;

    public async Task<CheckResult> RunAsync(CheckDescriptor d, CancellationToken ct)
    {
        var url = d.Parameters.TryGetValue("url", out var u) ? u : null;
        var timeoutMsStr = d.Parameters.TryGetValue("timeoutMs", out var t) ? t : "10000";
        if (string.IsNullOrWhiteSpace(url))
            return new CheckResult(d.Id, DateTimeOffset.UtcNow, CheckStatus.Unknown, "url missing");

        if (!int.TryParse(timeoutMsStr, out var timeoutMs)) timeoutMs = 10000;
        _http.Timeout = TimeSpan.FromMilliseconds(timeoutMs);

        var sw = Stopwatch.StartNew();
        try
        {
            using var resp = await _http.GetAsync(url, ct);
            sw.Stop();
            var ok = (int)resp.StatusCode < 400;
            var status = ok ? CheckStatus.Healthy : CheckStatus.Unhealthy;
            var metrics = new Dictionary<string,double> { ["latency_ms"] = sw.Elapsed.TotalMilliseconds };
            var dims = new Dictionary<string,string> { {"url", url}, {"status_code", ((int)resp.StatusCode).ToString()} };
            return new CheckResult(d.Id, DateTimeOffset.UtcNow, status, $"HTTP {(int)resp.StatusCode}", metrics, dims);
        }
        catch (Exception ex)
        {
            sw.Stop();
            var metrics = new Dictionary<string,double> { ["latency_ms"] = sw.Elapsed.TotalMilliseconds };
            return new CheckResult(d.Id, DateTimeOffset.UtcNow, CheckStatus.Unhealthy, $"Exception: {ex.Message}", metrics,
                new Dictionary<string,string>{{"url", url ?? ""}});
        }
    }
}
