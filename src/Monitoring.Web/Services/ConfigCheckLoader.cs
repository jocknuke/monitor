
using System.Text.Json;
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Services;

public class ConfigCheckLoader
{
    private readonly IConfiguration _cfg;
    private readonly ICheckStore _store;
    private readonly ILogger<ConfigCheckLoader> _log;
    private readonly IDisposable _reloadToken;

    public ConfigCheckLoader(IConfiguration cfg, ICheckStore store, ILogger<ConfigCheckLoader> log)
    {
        _cfg = cfg; _store = store; _log = log;
        _reloadToken = ChangeToken.OnChange(_cfg.GetReloadToken, async () => await ApplyFromConfigurationAsync());
    }

    public async Task ApplyFromConfigurationAsync()
    {
        var section = _cfg.GetSection("Checks");
        var list = new List<CheckDescriptor>();
        foreach (var child in section.GetChildren())
        {
            var id = child["Id"] ?? Guid.NewGuid().ToString("n");
            var type = child["Type"] ?? "unknown";
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
