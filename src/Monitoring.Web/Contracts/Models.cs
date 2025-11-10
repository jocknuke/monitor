
namespace Monitoring.Web.Contracts;

public enum CheckStatus { Healthy = 0, Degraded = 1, Unhealthy = 2, Unknown = 3 }

public record CheckDescriptor(
    string Id,
    string Type,
    string Name,
    TimeSpan Interval,
    IReadOnlyDictionary<string,string> Parameters,
    IReadOnlyList<string> Tags,
    bool Enabled = true);

public record CheckResult(
    string CheckId,
    DateTimeOffset ObservedAt,
    CheckStatus Status,
    string? Message,
    IReadOnlyDictionary<string,double>? Metrics = null,
    IReadOnlyDictionary<string,string>? Dimensions = null);

[AttributeUsage(AttributeTargets.Class)]
public sealed class CheckAttribute : Attribute
{
    public string Type { get; }
    public CheckAttribute(string type) => Type = type;
}

public interface ICheck
{
    Task<CheckResult> RunAsync(CheckDescriptor descriptor, CancellationToken ct);
}
