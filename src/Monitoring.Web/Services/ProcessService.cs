using System;
using System.Collections.Generic;
using System.Linq;
using Monitoring.Web.Models;

namespace Monitoring.Web.Services
{
    /// <summary>
    /// Provides access to a set of sample nightly batch processes and their associated
    /// details. This service is registered as a singleton so that sample data
    /// persists for the lifetime of the application. In a real application
    /// processes would likely be pulled from a database or monitoring pipeline.
    /// </summary>
    public class ProcessService
    {
        private readonly List<ProcessDetail> _processes;

        public ProcessService()
        {
            // Seed some sample processes to demonstrate the UI. In a real
            // application these would be loaded from a database or monitoring pipeline.
            _processes = new List<ProcessDetail>
            {
                new ProcessDetail
                {
                    Name = "Nightly ETL",
                    Completed = true,
                    LastRun = DateTime.Now.Date.AddDays(-1).AddHours(1),
                    DurationMinutes = 75,
                    ExpectedMinMinutes = 60,
                    ExpectedMaxMinutes = 90,
                    DurationHistory = new List<double> {70, 72, 75, 80, 73, 74, 76},
                    Tags = new List<string> { "ETL", "batch", "prod" },
                    Apis = new List<ApiStatus>
                    {
                        new ApiStatus { Name = "Auth API", Healthy = true, ResponseTimeMs = 200 },
                        new ApiStatus { Name = "Payment API", Healthy = true, ResponseTimeMs = 350 },
                    },
                    Databases = new List<DbConnectionStatus>
                    {
                        new DbConnectionStatus { Name = "OrdersDB", Connected = true, LatencyMs = 18 },
                        new DbConnectionStatus { Name = "CustomersDB", Connected = true, LatencyMs = 22 },
                    },
                    ServiceAccounts = new List<ServiceAccountStatus>
                    {
                        new ServiceAccountStatus { Name = "etl_account", HasAccess = true },
                        new ServiceAccountStatus { Name = "reporting_account", HasAccess = true },
                    }
                },
                new ProcessDetail
                {
                    Name = "Nightly Reconciliation",
                    Completed = false,
                    LastRun = DateTime.Now.Date.AddDays(-1).AddHours(2),
                    DurationMinutes = 120,
                    ExpectedMinMinutes = 90,
                    ExpectedMaxMinutes = 110,
                    DurationHistory = new List<double> {100, 95, 105, 110, 115, 120, 125},
                    Tags = new List<string> { "recon", "batch" },
                    Apis = new List<ApiStatus>
                    {
                        new ApiStatus { Name = "Inventory API", Healthy = false, ResponseTimeMs = 600 },
                    },
                    Databases = new List<DbConnectionStatus>
                    {
                        new DbConnectionStatus { Name = "InventoryDB", Connected = true, LatencyMs = 25 },
                    },
                    ServiceAccounts = new List<ServiceAccountStatus>
                    {
                        new ServiceAccountStatus { Name = "recon_account", HasAccess = false },
                    }
                }
            };
        }

        /// <summary>
        /// Returns a lightweight summary list of all processes. Each summary includes
        /// last run and duration statistics for display in tables.
        /// </summary>
        public IEnumerable<ProcessSummary> GetProcesses()
        {
            return _processes.Select(p => new ProcessSummary
            {
                Name = p.Name,
                Completed = p.Completed,
                LastRun = p.LastRun,
                DurationMinutes = p.DurationMinutes,
                ExpectedMinMinutes = p.ExpectedMinMinutes,
                ExpectedMaxMinutes = p.ExpectedMaxMinutes,
                DurationHistory = p.DurationHistory,
                Tags = p.Tags
            });
        }

        /// <summary>
        /// Retrieves the detailed representation of a process by name. Returns null
        /// if no matching process exists.
        /// </summary>
        public ProcessDetail? GetProcess(string name)
        {
            return _processes.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}