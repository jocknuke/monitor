
using Dapper;
using Microsoft.Data.SqlClient;

namespace Monitoring.Web.Services;

public class SqlJobService
{
    private const string HistorySql = @"
;WITH hist AS (
  SELECT TOP (@take)
         j.name as JobName,
         h.run_status,
         CONVERT(datetime2,
           STUFF(STUFF(RIGHT('00000000' + CAST(h.run_date AS varchar(8)),8),5,0,'-'),8,0,'-') + ' ' +
           STUFF(STUFF(RIGHT('000000' + CAST(h.run_time AS varchar(6)),6),3,0,':'),6,0,':')) AS RunAt,
         ((h.run_duration/10000)*3600 + ((h.run_duration%10000)/100)*60 + (h.run_duration%100)) AS DurationSec,
         h.message
  FROM msdb.dbo.sysjobs j
  LEFT JOIN msdb.dbo.sysjobhistory h ON h.job_id = j.job_id AND h.step_id = 0
  WHERE j.name = @jobName
  ORDER BY h.instance_id DESC
)
SELECT * FROM hist ORDER BY RunAt ASC;";

    private const string SummarySql = @"
SELECT j.name as JobName, j.enabled,
       MAX(CASE WHEN h.step_id=0 THEN h.run_status END) as LastRunStatus,
       MAX(CASE WHEN h.step_id=0 THEN CONVERT(datetime2,
            STUFF(STUFF(RIGHT('00000000' + CAST(h.run_date AS varchar(8)),8),5,0,'-'),8,0,'-') + ' ' +
            STUFF(STUFF(RIGHT('000000' + CAST(h.run_time AS varchar(6)),6),3,0,':'),6,0,':')) END) as LastRunAt
FROM msdb.dbo.sysjobs j
LEFT JOIN msdb.dbo.sysjobhistory h ON h.job_id = j.job_id
WHERE j.name = @jobName
GROUP BY j.name, j.enabled;";

    public async Task<(bool exists, int? lastStatus, DateTimeOffset? lastRunAt, List<(DateTimeOffset at, int durationSec, int status)> history)> 
        GetJobSummaryAndHistoryAsync(string connectionString, string jobName, int take = 30, CancellationToken ct = default)
    {
        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);

        var summary = await conn.QueryFirstOrDefaultAsync(SummarySql, new { jobName });
        var hist = (await conn.QueryAsync(HistorySql, new { jobName, take })).ToList();

        bool exists = summary != null;
        int? lastStatus = summary?.LastRunStatus;
        DateTimeOffset? lastRunAt = summary?.LastRunAt;

        var history = new List<(DateTimeOffset at, int durationSec, int status)>();
        for (int i=0;i<hist.Count;i++) {
            var h = hist[i];
            history.Add(( (DateTimeOffset)h.RunAt, (int)h.DurationSec, (int)(h.run_status ?? 3) ));
        }

        return (exists, lastStatus, lastRunAt, history);
    }
}
