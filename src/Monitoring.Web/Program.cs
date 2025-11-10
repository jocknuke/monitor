using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using Monitoring.Web.Services;
using Monitoring.Web.Contracts;
using Monitoring.Web.Checks;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// Antiforgery configuration
//
// Razor Components with interactive server render mode require antiforgery
// support. Without antiforgery middleware, endpoints annotated with
// antiforgery metadata (e.g. those that handle interactive form posts) will
// throw InvalidOperationException at runtime. Register antiforgery services
// and add the middleware below to avoid runtime exceptions.
builder.Services.AddAntiforgery();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
// Register DashboardService to provide data for the operations dashboard.  This
// service supplies summary tiles, job trends, services and alerts for the main page.
builder.Services.AddSingleton<DashboardService>();


// -----------------------------------------------------------------------------
// Monitoring services and scheduler configuration
//
// Register SignalR so that results can be streamed to connected clients.
builder.Services.AddSignalR();

// Register in-memory stores for check descriptors and results. These can be
// replaced with durable implementations (e.g. EF Core, DynamoDB) without
// changing the rest of the code. The default max items for the result store
// determines how many historical results are retained per check.
builder.Services.AddSingleton<ICheckStore, InMemoryCheckStore>();
builder.Services.AddSingleton<IResultStore>(sp => new InMemoryResultStore(maxItems: 200));

// Register the registry and individual checks. The registry discovers
// implementations via the CheckAttribute and provides them to the scheduler.
builder.Services.AddSingleton<ICheckRegistry, CheckRegistry>();
builder.Services.AddSingleton<ICheck, SamplePingCheck>();
builder.Services.AddSingleton<ICheck, SqlAgentJobCheck>();

// Register the scheduler as a hosted service so that checks are executed
// periodically in the background. The scheduler depends on the stores,
// registry and SignalR hub context.
builder.Services.AddHostedService<Scheduler>();

// Register HttpClient for HTTP checks. HttpEndpointCheck depends on HttpClient
builder.Services.AddHttpClient();

// Register additional checks for HTTP endpoints and DB connections
builder.Services.AddSingleton<ICheck, HttpEndpointCheck>();
builder.Services.AddSingleton<ICheck, DbConnectionCheck>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add antiforgery middleware between routing and endpoint mapping. This is
// required for endpoints that use antiforgery metadata (e.g. interactive
// components). Without this call, an InvalidOperationException will be
// thrown at runtime when the framework processes requests for '/' or other
// endpoints annotated with antiforgery metadata.
app.UseAntiforgery();

// Map SignalR hub for streaming check results. Clients connect to this path
// to receive real-time updates when checks are executed.
app.MapHub<ResultsHub>("/results");

app.MapRazorComponents<Monitoring.Web.App>()
    .AddInteractiveServerRenderMode();

app.Run();
