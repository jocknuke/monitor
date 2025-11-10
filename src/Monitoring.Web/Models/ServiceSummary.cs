namespace Monitoring.Web.Models;

public class ServiceSummary
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Healthy";
    public int P95ms { get; set; }
    public double ErrorRate { get; set; }
    public List<int> LastLatencies { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}
