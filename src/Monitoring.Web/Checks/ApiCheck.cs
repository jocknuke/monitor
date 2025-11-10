
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Checks;

[Check("api")]
public class ApiCheck : ICheck
{
    private readonly HttpClient _http;
    public ApiCheck(HttpClient http) => _http = http;

    public async Task<CheckResult> RunAsync(CheckDescriptor d, CancellationToken ct)
    {
        var url = d.Parameters.GetValueOrDefault("url");
        var method = d.Parameters.GetValueOrDefault("method") ?? "GET";
        var headersJson = d.Parameters.GetValueOrDefault("headers") ?? "{}";
        var body = d.Parameters.GetValueOrDefault("body") ?? "";
        var expectContains = d.Parameters.GetValueOrDefault("expectContains");

        if (string.IsNullOrWhiteSpace(url))
            return new CheckResult(d.Id, DateTimeOffset.UtcNow, CheckStatus.Unknown, "url missing");

        using var req = new HttpRequestMessage(new HttpMethod(method), url);
        try
        {
            var headers = JsonSerializer.Deserialize<Dictionary<string,string>>(headersJson) ?? new();
            foreach (var kv in headers) req.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
        }
        catch { /* ignore bad headers */ }

        if (!string.IsNullOrEmpty(body) && method.ToUpperInvariant() != "GET")
            req.Content = new StringContent(body, Encoding.UTF8, "application/json");

        var sw = Stopwatch.StartNew();
        try
        {
            using var resp = await _http.SendAsync(req, ct);
            sw.Stop();
            var text = await resp.Content.ReadAsStringAsync(ct);
            var ok = (int)resp.StatusCode < 400;
            if (ok && !string.IsNullOrWhiteSpace(expectContains) && text?.Contains(expectContains, StringComparison.OrdinalIgnoreCase) == false)
                ok = false;
            var status = ok ? CheckStatus.Healthy : CheckStatus.Unhealthy;
            var metrics = new Dictionary<string,double> { ["latency_ms"] = sw.Elapsed.TotalMilliseconds };
            var dims = new Dictionary<string,string> { {"url", url}, {"status_code", ((int)resp.StatusCode).ToString()} };
            return new CheckResult(d.Id, DateTimeOffset.UtcNow, status, $"HTTP {(int)resp.StatusCode}", metrics, dims);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new CheckResult(d.Id, DateTimeOffset.UtcNow, CheckStatus.Unhealthy, $"Exception: {ex.Message}",
                new Dictionary<string,double> { ["latency_ms"] = sw.Elapsed.TotalMilliseconds });
        }
    }
}
