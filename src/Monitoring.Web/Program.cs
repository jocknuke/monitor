
using Microsoft.AspNetCore.Components;
using MudBlazor.Services;
using Monitoring.Web.Services;
using Monitoring.Web.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Bind checks from configuration with reloadOnChange
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Blazor + Mud
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();
builder.Services.AddAntiforgery();

// Core stores and registry
builder.Services.AddSingleton<ICheckStore, InMemoryCheckStore>();
builder.Services.AddSingleton<IResultStore, InMemoryResultStore>();
builder.Services.AddSingleton<ICheckRegistry, CheckRegistry>();
builder.Services.AddSingleton<SqlJobService>();
builder.Services.AddHttpClient();

// Register built-in checks (plugin style via attribute type keys)
builder.Services.AddSingleton<ICheck, Monitoring.Web.Checks.SqlAgentJobCheck>();
builder.Services.AddSingleton<ICheck, Monitoring.Web.Checks.HttpEndpointCheck>();
builder.Services.AddSingleton<ICheck, Monitoring.Web.Checks.DbConnectionCheck>();

// Optional SignalR hub for push
builder.Services.AddSignalR();

// Scheduler + config loader
builder.Services.AddSingleton<ConfigCheckLoader>();
builder.Services.AddHostedService<CheckSchedulerService>();

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
app.MapHub<ResultsHub>("/hubs/results");
app.MapFallbackToPage("/_Host");

// Seed from config initially
using (var scope = app.Services.CreateScope())
{
    var loader = scope.ServiceProvider.GetRequiredService<ConfigCheckLoader>();
    await loader.ApplyFromConfigurationAsync();
}

app.Run();
