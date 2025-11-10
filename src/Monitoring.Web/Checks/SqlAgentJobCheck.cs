using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Checks
{
    /// <summary>
    /// Example check that monitors SQL Server Agent job execution history. This check
    /// is provided as a skeleton and will return Unknown because a SQL connection
    /// is not configured in the sample project. To enable, inject Microsoft.Data.SqlClient
    /// and implement the logic to query msdb.sysjobhistory and related tables.
    /// </summary>
    [Check("sql-agent-job")]
    public class SqlAgentJobCheck : ICheck
    {
        public Task<CheckResult> RunAsync(CheckDescriptor descriptor, CancellationToken ct)
        {
            // This sample does not implement actual SQL Agent monitoring. If you
            // have a SQL connection string in descriptor.Parameters, you can use
            // Microsoft.Data.SqlClient to query msdb.dbo.sysjobs and sysjobhistory
            // to determine the last outcome of each job matching descriptor.Parameters["jobNamePattern"].
            var result = new CheckResult(
                descriptor.Id,
                DateTimeOffset.UtcNow,
                CheckStatus.Unknown,
                "SQL Agent monitoring not implemented in this sample.");
            return Task.FromResult(result);
        }
    }
}