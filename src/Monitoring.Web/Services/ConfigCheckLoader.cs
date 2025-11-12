using System.Text.Json;
using Microsoft.Extensions.Primitives;
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Services;

public class ConfigCheckLoader
{
    private readonly IConfiguration _cfg;
    private readonly ICheckStore _store;
    private readonly ILogger<ConfigCheckLoader> _log;
    private IDisposable? _reloadDisposable;

    public ConfigCheckLoader(IConfiguration cfg, ICheckStore store, ILogger<ConfigCheckLoader> log)
    {
        _cfg = cfg; _store = store; _log = log;
        _reloadDisposable = ChangeToken.OnChange(
            () => _cfg.GetReloadToken(),
            async () =>
            {
                try { await ApplyFromConfigurationAsync(); }
                catch (Exception ex) { _log.LogError(ex, "Failed to reload checks from configuration"); }
            });
    }

    public async Task ApplyFromConfigurationAsync()
    {
        var list = new List<CheckDescriptor>();
        var section = _cfg.GetSection("Checks");
        foreach (var child in section.GetChildren())
        {
            var id = child["Id"] ?? child.Key;
            var type = child["Type"] ?? "custom";
            var name = child["Name"] ?? id;
            var cron = child["Cron"];
            int? intervalSeconds = int.TryParse(child["IntervalSeconds"], out var s) ? s : null;
            var enabled = bool.TryParse(child["Enabled"], out var en) ? en : true;
            var tags = child.GetSection("Tags").Get<string[]>() ?? Array.Empty<string>();
            var parameters = child.GetSection("Parameters").Get<Dictionary<string,string>>() ?? new();

            var d = new CheckDescriptor(id, type, name, cron, intervalSeconds, parameters, tags, enabled);
            list.Add(d);
        }
        await _store.UpsertManyAsync(list);
        _log.LogInformation("Loaded {Count} checks from configuration", list.Count);
    }
}
