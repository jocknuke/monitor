
# MonitoringStarter – All Checks (Fixed) (.NET 9, Blazor Server, MudBlazor)

**Pluggable checks** + lightweight scheduler + detail page:
- `sql-agent-job` – MSDB last status + history (line chart)
- `http` – simple GET with latency metric
- `db-connection` – opens SQL connection, optional test query
- `api` – method/headers/body + contains expectation
- `serviceaccount-locks` – checks SQL login lock/disable state

## Configure
Edit `src/Monitoring.Web/appsettings.json` → `ConnectionStrings:MonitoringDb` to your SQL Server/MSDB.

## Run
```bash
dotnet restore
dotnet build
cd src/Monitoring.Web
dotnet run
```
Open `http://localhost:5000` → Overview → click **ETL: Daily Sales** for SQL job history.
