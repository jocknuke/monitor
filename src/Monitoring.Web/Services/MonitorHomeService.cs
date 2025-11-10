using Monitoring.Web.Models;

namespace Monitoring.Web.Services;

public class MonitorHomeService
{
    public int UpCount => 12;
    public int DegradedCount => 3;
    public int DownCount => 1;
    public int SilencedCount => 0;

    public IEnumerable<Incident> Incidents => new List<Incident>
    {
        new Incident
        {
            Title = "P1 - API Gateway Down",
            Severity = IncidentSeverity.P1,
            ServiceName = "API Gateway",
            OpenedAt = DateTime.Now.AddMinutes(-30).ToString("HH:mm")
        },
        new Incident
        {
            Title = "P2 - SQL Latency Spike",
            Severity = IncidentSeverity.P2,
            ServiceName = "SQL Database",
            OpenedAt = DateTime.Now.AddMinutes(-15).ToString("HH:mm")
        },
        new Incident
        {
            Title = "P3 - Cache Misses High",
            Severity = IncidentSeverity.P3,
            ServiceName = "Redis Cache",
            OpenedAt = DateTime.Now.AddMinutes(-5).ToString("HH:mm")
        }
    };

    public IEnumerable<ServiceSummary> Services => new List<ServiceSummary>
    {
        new ServiceSummary
        {
            Name = "SQL Database",
            Status = "Degraded",
            P95ms = 420,
            ErrorRate = 0.007,
            LastLatencies = new List<int> { 300, 350, 420, 380, 390 },
            Tags = new List<string> { "prod", "database" }
        },
        new ServiceSummary
        {
            Name = "API Gateway",
            Status = "Healthy",
            P95ms = 95,
            ErrorRate = 0.001,
            LastLatencies = new List<int> { 80, 85, 90, 95, 85 },
            Tags = new List<string> { "prod", "web" }
        }
    };
}
