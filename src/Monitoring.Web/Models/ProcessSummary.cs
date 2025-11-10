using System;
using System.Collections.Generic;

namespace Monitoring.Web.Models
{
    /// <summary>
    /// Represents a summary of a nightly or batch-like process. A process has a name,
    /// completion status, last run information, run duration and a historical range for
    /// acceptable durations. The Status property categorizes the process into
    /// Healthy, Degraded or Unhealthy based on whether the last run completed and
    /// whether the duration falls within the expected range.
    /// </summary>
    public class ProcessSummary
    {
        /// <summary>
        /// Name of the process (unique key used in URLs).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the most recent execution completed successfully.
        /// </summary>
        public bool Completed { get; set; } = true;

        /// <summary>
        /// Timestamp of the last execution.
        /// </summary>
        public DateTime LastRun { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Duration of the last run, in minutes.
        /// </summary>
        public double DurationMinutes { get; set; } = 0;

        /// <summary>
        /// Expected minimum duration in minutes for the process. Values below this threshold
        /// may indicate abnormal behaviour (e.g. skipped work).
        /// </summary>
        public double ExpectedMinMinutes { get; set; } = 0;

        /// <summary>
        /// Expected maximum duration in minutes for the process. Values above this threshold
        /// may indicate performance degradation.
        /// </summary>
        public double ExpectedMaxMinutes { get; set; } = 0;

        /// <summary>
        /// Historical run durations (in minutes) used to render trend charts.
        /// </summary>
        public List<double> DurationHistory { get; set; } = new();

        /// <summary>
        /// Arbitrary tags used for filtering or grouping processes.
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Computes a health status based on completion and whether the duration falls within
        /// the expected min/max range.
        /// </summary>
        public string Status
        {
            get
            {
                if (!Completed)
                {
                    return "Unhealthy";
                }
                if (DurationMinutes < ExpectedMinMinutes || DurationMinutes > ExpectedMaxMinutes)
                {
                    return "Degraded";
                }
                return "Healthy";
            }
        }
    }
}