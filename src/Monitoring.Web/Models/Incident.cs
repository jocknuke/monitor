namespace Monitoring.Web.Models;

public enum IncidentSeverity { P1, P2, P3 }

public class Incident
{
    public string Title { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string OpenedAt { get; set; } = string.Empty;
}
