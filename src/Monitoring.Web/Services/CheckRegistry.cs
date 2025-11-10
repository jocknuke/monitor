
using System.Reflection;
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Services;

public interface ICheckRegistry
{
    ICheck? Resolve(string type);
    IEnumerable<string> RegisteredTypes { get; }
}

public class CheckRegistry : ICheckRegistry
{
    private readonly IServiceProvider _sp;
    private readonly Dictionary<string, Type> _map;
    public CheckRegistry(IServiceProvider sp)
    {
        _sp = sp;
        _map = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ICheck).IsAssignableFrom(t) && !t.IsAbstract)
            .Select(t => new { t, attr = t.GetCustomAttribute<CheckAttribute>() })
            .Where(x => x.attr is not null)
            .ToDictionary(x => x!.attr!.Type, x => x!.t, StringComparer.OrdinalIgnoreCase);
    }

    public ICheck? Resolve(string type) => _map.TryGetValue(type, out var t) ? (ICheck?)_sp.GetService(t)! : null;
    public IEnumerable<string> RegisteredTypes => _map.Keys;
}
