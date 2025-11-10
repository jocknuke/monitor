namespace Monitoring.Web.Models;

public enum CheckStatus { Healthy, Degraded, Unhealthy, Unknown }

public class CheckResultRow
{
    public DateTime ObservedAt { get; set; }
    public string CheckName { get; set; } = string.Empty;
    public CheckStatus Status { get; set; }
    public int LatencyMs { get; set; }
    public string Message { get; set; } = string.Empty;
}
