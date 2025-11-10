
# MonitoringStarter – All Checks (.NET 9, Blazor Server, MudBlazor)

**Pluggable checks** with a lightweight scheduler + detail page:
- `sql-agent-job` – reads MSDB to get last status and history (line chart)
- `http` – simple GET with latency metric
- `db-connection` – connects to DB and optionally runs a test query
- `api` – customizable method/headers/body and content expectation
- `serviceaccount-locks` – queries `sys.sql_logins` for lock/disable status

## Configure
Edit `src/Monitoring.Web/appsettings.json` → `ConnectionStrings:MonitoringDb` to your SQL Server/MSDB.

## Run
```bash
dotnet restore
dotnet build
cd src/Monitoring.Web
dotnet run
```
Open `http://localhost:5000` for the overview. Click **ETL: Daily Sales** for SQL job history.

## Extend
Add a new class implementing `ICheck` and decorate with `[Check("your-type")]`. Add parameters to a `CheckDescriptor` and it will be scheduled automatically.
