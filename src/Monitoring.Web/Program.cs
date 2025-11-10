
using Microsoft.AspNetCore.Components;
using MudBlazor.Services;
using Monitoring.Web.Services;
using Monitoring.Web.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();
builder.Services.AddHttpClient();

// Core stores and registry
builder.Services.AddSingleton<ICheckStore, InMemoryCheckStore>();
builder.Services.AddSingleton<IResultStore, InMemoryResultStore>();
builder.Services.AddSingleton<ICheckRegistry, CheckRegistry>();

// Domain services
builder.Services.AddSingleton<SqlJobService>();

// Register checks
builder.Services.AddSingleton<ICheck, Monitoring.Web.Checks.SqlAgentJobCheck>();
builder.Services.AddSingleton<ICheck, Monitoring.Web.Checks.HttpEndpointCheck>();
builder.Services.AddSingleton<ICheck, Monitoring.Web.Checks.DbConnectionCheck>();
builder.Services.AddSingleton<ICheck, Monitoring.Web.Checks.ApiCheck>();
builder.Services.AddSingleton<ICheck, Monitoring.Web.Checks.ServiceAccountLockCheck>();

// Scheduler
builder.Services.AddHostedService<CheckSchedulerService>();

// Antiforgery for interactive endpoints
builder.Services.AddAntiforgery();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

Seed.SeedChecks(app.Services);

app.Run();

internal static class Seed
{
    public static void SeedChecks(IServiceProvider sp)
    {
        var cfg = sp.GetRequiredService<IConfiguration>();
        var store = sp.GetRequiredService<ICheckStore>();
        var conn = cfg.GetConnectionString("MonitoringDb") ?? "";

        // 1) SQL Agent job
        var sqlJob = new CheckDescriptor(
            Id: "etl-daily-sales",
            Type: "sql-agent-job",
            Name: "ETL: Daily Sales",
            Interval: TimeSpan.FromMinutes(2),
            Parameters: new Dictionary<string,string> {
                {"connectionString", conn},
                {"jobName", "ETL: Daily Sales"},
                {"historyCount", "30"}
            },
            Tags: new[]{"nightly","sales"}
        );

        // 2) Public HTTP endpoint
        var http = new CheckDescriptor(
            Id: "http-github",
            Type: "http",
            Name: "HTTP: Github API",
            Interval: TimeSpan.FromMinutes(1),
            Parameters: new Dictionary<string,string> {
                {"url","https://api.github.com/"},
                {"timeoutMs","8000"}
            },
            Tags: new[]{"http","public"}
        );

        // 3) DB connection
        var db = new CheckDescriptor(
            Id: "db-primary",
            Type: "db-connection",
            Name: "DB: Primary",
            Interval: TimeSpan.FromMinutes(2),
            Parameters: new Dictionary<string,string> {
                {"connectionString", conn},
                {"testQuery","SELECT 1"}
            },
            Tags: new[]{"database"}
        );

        // 4) API check with method/headers and JSONPath-like contains
        var api = new CheckDescriptor(
            Id: "api-status",
            Type: "api",
            Name: "API: Status",
            Interval: TimeSpan.FromMinutes(1),
            Parameters: new Dictionary<string,string> {
                {"url","https://httpbin.org/anything"},
                {"method","GET"},
                {"expectContains","url"}
            },
            Tags: new[]{"api"}
        );

        // 5) Service account lock check (SQL login example)
        var svcAcct = new CheckDescriptor(
            Id: "svc-locks",
            Type: "serviceaccount-locks",
            Name: "Service Accounts: Lock Status",
            Interval: TimeSpan.FromMinutes(5),
            Parameters: new Dictionary<string,string> {
                {"connectionString", conn},
                {"login","svc_etl_runner"}
            },
            Tags: new[]{"security"}
        );

        foreach (var d in new[]{sqlJob, http, db, api, svcAcct})
            store.UpsertAsync(d).GetAwaiter().GetResult();
    }
}
