using System.Collections.Generic;
using Monitoring.Web.Models;

namespace Monitoring.Web.Services
{
    /// <summary>
    /// Provides sample data for the IT operations dashboard.  In a real
    /// application this service would query monitoring back ends or
    /// databases to populate the collections with real‑time values.
    /// </summary>
    public class DashboardService
    {
        public IList<SummaryInfo> Summaries { get; }
        public IList<JobTrendInfo> JobTrends { get; }
        public IList<ServiceInfo> Services { get; }
        public IList<AlertInfo> Alerts { get; }

        public DashboardService()
        {
            // Populate summary metrics.  The values here mirror those shown
            // in the provided mockup image: a fraction of healthy items out of
            // total items plus an arbitrary change percentage relative to the
            // prior period.
            Summaries = new List<SummaryInfo>
            {
                new SummaryInfo { Title = "Jobs",      Healthy = 1, Total = 3, ChangePercent = 0 },
                new SummaryInfo { Title = "APIs",      Healthy = 2, Total = 3, ChangePercent = -5 },
                new SummaryInfo { Title = "Databases", Healthy = 2, Total = 2, ChangePercent = 0 },
                new SummaryInfo { Title = "Accounts",  Healthy = 1, Total = 2, ChangePercent = -50 },
                new SummaryInfo { Title = "Servers",   Healthy = 2, Total = 3, ChangePercent = 0 },
                new SummaryInfo { Title = "Open Tickets", Healthy = 8, Total = 8, ChangePercent = -33 },
            };

            // Populate nightly job trends.  Durations are in minutes and will
            // be plotted on a line chart; smaller arrays correspond to last
            // seven runs.  Status values determine the chip colour in the
            // dashboard.
            JobTrends = new List<JobTrendInfo>
            {
                new JobTrendInfo
                {
                    Name = "Daily Sales ETL",
                    LastRun = "2m 25s",
                    Median = "2m 28s",
                    SuccessRate = 98.5,
                    Status = JobStatus.Ok,
                    Durations = new List<double> { 2.1, 2.4, 2.3, 2.2, 2.5, 2.4, 2.3 }
                },
                new JobTrendInfo
                {
                    Name = "Customer Sync",
                    LastRun = "7m 30s",
                    Median = "3m 18s",
                    SuccessRate = 95.2,
                    Status = JobStatus.Warning,
                    Durations = new List<double> { 4.0, 3.5, 3.2, 4.8, 5.0, 3.3, 3.6 }
                },
                new JobTrendInfo
                {
                    Name = "Inventory Update",
                    LastRun = "0m 0s",
                    Median = "2m 55s",
                    SuccessRate = 92.8,
                    Status = JobStatus.Error,
                    Durations = new List<double> { 3.0, 2.8, 3.1, 2.6, 2.9, 3.4, 2.7 }
                },
            };

            // Populate service table rows.  These mirror the services shown in the
            // sample mockup and include environment, team and SLA status.
            Services = new List<ServiceInfo>
            {
                new ServiceInfo
                {
                    Name = "Daily Sales ETL",
                    Type = "Job",
                    Status = ServiceStatus.Ok,
                    Environment = "Production",
                    Team = "Data Engineering",
                    LastCheck = "18 minutes ago",
                    SLA = "Meeting"
                },
                new ServiceInfo
                {
                    Name = "Customer Sync",
                    Type = "Job",
                    Status = ServiceStatus.Warning,
                    Environment = "Production",
                    Team = "Customer Success",
                    LastCheck = "about 1 hour ago",
                    SLA = "At-Risk"
                },
                new ServiceInfo
                {
                    Name = "Inventory Update",
                    Type = "Job",
                    Status = ServiceStatus.Error,
                    Environment = "Production",
                    Team = "Operations",
                    LastCheck = "8 minutes ago",
                    SLA = "Breached"
                },
                new ServiceInfo
                {
                    Name = "Payment Gateway API",
                    Type = "API",
                    Status = ServiceStatus.Ok,
                    Environment = "Production",
                    Team = "Payments",
                    LastCheck = "3 minutes ago",
                    SLA = "Meeting"
                },
                new ServiceInfo
                {
                    Name = "User Service API",
                    Type = "API",
                    Status = ServiceStatus.Ok,
                    Environment = "Production",
                    Team = "Platform",
                    LastCheck = "3 minutes ago",
                    SLA = "Meeting"
                },
            };

            // Populate recent alerts.  Each alert has a severity and message
            // describing the cause.  TimeAgo conveys when the alert occurred.
            Alerts = new List<AlertInfo>
            {
                new AlertInfo
                {
                    Name = "Inventory Update",
                    Severity = AlertSeverity.Critical,
                    Message = "Job failed: connection timeout to warehouse database",
                    TimeAgo = "5 minutes ago",
                    IsAcknowledged = false,
                    User = null
                },
                new AlertInfo
                {
                    Name = "API Service Account",
                    Severity = AlertSeverity.Critical,
                    Message = "Account locked due to multiple failed login attempts",
                    TimeAgo = "1 hour ago",
                    IsAcknowledged = false,
                    User = null
                },
                new AlertInfo
                {
                    Name = "Customer Sync",
                    Severity = AlertSeverity.Warning,
                    Message = "Job duration exceeded baseline by 150%",
                    TimeAgo = "about 1 hour ago",
                    IsAcknowledged = true,
                    User = "john.doe@company.com"
                },
            };
        }
    }
}