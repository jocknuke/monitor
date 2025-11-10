using System;
using System.Collections.Generic;

namespace Monitoring.Web.Models
{
    /// <summary>
    /// Represents a high‑level summary metric such as Jobs or APIs.  Each summary
    /// includes a title, the number of healthy instances, the total number of
    /// instances, and the percentage of healthy instances.  A change percentage
    /// indicates how the metric has shifted since the previous period.
    /// </summary>
    public class SummaryInfo
    {
        public string Title { get; set; } = string.Empty;
        public int Healthy { get; set; }
        public int Total { get; set; }
        public double PercentHealthy => Total == 0 ? 0.0 : (double)Healthy / Total * 100.0;
        public double ChangePercent { get; set; }
    }

    /// <summary>
    /// Indicates the health of an individual job trend.  OK means the job is
    /// operating within expectations, Warning indicates potential issues such as
    /// degraded performance or sporadic failures, and Error indicates that the
    /// job has failed or is unhealthy.
    /// </summary>
    public enum JobStatus
    {
        Ok,
        Warning,
        Error
    }

    /// <summary>
    /// Holds metadata and trend information for a nightly or scheduled job.
    /// Durations contains the last N run durations (in minutes) and is used to
    /// plot a small line chart.  Other fields summarise the last run time,
    /// median duration and success rate.
    /// </summary>
    public class JobTrendInfo
    {
        public string Name { get; set; } = string.Empty;
        public string LastRun { get; set; } = string.Empty;
        public string Median { get; set; } = string.Empty;
        public double SuccessRate { get; set; }
        public JobStatus Status { get; set; }
        public List<double> Durations { get; set; } = new();
    }

    /// <summary>
    /// Indicates the status of a monitored service or API.  Values mirror the
    /// high‑level status semantics used elsewhere in the dashboard.
    /// </summary>
    public enum ServiceStatus
    {
        Ok,
        Warning,
        Error
    }

    /// <summary>
    /// Represents a row in the services table.  Includes metadata such as
    /// environment, team and SLA status in addition to basic health state.
    /// </summary>
    public class ServiceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public ServiceStatus Status { get; set; }
        public string Environment { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public string LastCheck { get; set; } = string.Empty;
        public string SLA { get; set; } = string.Empty;
    }

    /// <summary>
    /// Severity levels for recent alerts.  Critical alerts require immediate
    /// attention, Warning alerts indicate a deteriorating condition, and Info
    /// alerts are informational only.
    /// </summary>
    public enum AlertSeverity
    {
        Critical,
        Warning,
        Info
    }

    /// <summary>
    /// Represents a single alert entry in the alerts panel.  Includes the
    /// associated service or job name, the severity, a short message, the
    /// relative time the alert occurred, and acknowledgement metadata.
    /// </summary>
    public class AlertInfo
    {
        public string Name { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public bool IsAcknowledged { get; set; }
        public string? User { get; set; }
    }
}