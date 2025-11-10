using System.Collections.Generic;

namespace Monitoring.Web.Models
{
    /// <summary>
    /// Represents a detailed view of a process including associated API health,
    /// database connection status and service account access. Inherits from
    /// ProcessSummary for basic properties.
    /// </summary>
    public class ProcessDetail : ProcessSummary
    {
        /// <summary>
        /// List of APIs associated with this process and their current health.
        /// </summary>
        public List<ApiStatus> Apis { get; set; } = new();

        /// <summary>
        /// List of databases connections used by this process and their status.
        /// </summary>
        public List<DbConnectionStatus> Databases { get; set; } = new();

        /// <summary>
        /// List of service accounts and whether they currently have access.
        /// </summary>
        public List<ServiceAccountStatus> ServiceAccounts { get; set; } = new();
    }

    /// <summary>
    /// Describes an API used by a process and whether it is healthy along with
    /// current response time in milliseconds.
    /// </summary>
    public class ApiStatus
    {
        public string Name { get; set; } = string.Empty;
        public bool Healthy { get; set; } = true;
        public double ResponseTimeMs { get; set; } = 0;
    }

    /// <summary>
    /// Represents the status of a database connection required by a process.
    /// </summary>
    public class DbConnectionStatus
    {
        public string Name { get; set; } = string.Empty;
        public bool Connected { get; set; } = true;
        public double LatencyMs { get; set; } = 0;
    }

    /// <summary>
    /// Represents a service account and whether it currently has required access.
    /// </summary>
    public class ServiceAccountStatus
    {
        public string Name { get; set; } = string.Empty;
        public bool HasAccess { get; set; } = true;
    }
}