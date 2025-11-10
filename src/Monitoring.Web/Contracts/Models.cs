using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Monitoring.Web.Contracts
{
    /// <summary>
    /// Represents the health status of a check execution.
    /// </summary>
    public enum CheckStatus
    {
        Healthy = 0,
        Degraded = 1,
        Unhealthy = 2,
        Unknown = 3
    }

    /// <summary>
    /// Metadata describing a scheduled check. A descriptor contains the identity,
    /// type, name, run interval and any parameters needed by the check implementation.
    /// </summary>
    public record CheckDescriptor(
        string Id,
        string Type,
        string Name,
        TimeSpan Interval,
        IDictionary<string, string> Parameters,
        IReadOnlyList<string> Tags,
        bool Enabled = true);

    /// <summary>
    /// The result of executing a check. In addition to the status and message,
    /// arbitrary numeric metrics and dimension metadata can be attached.
    /// </summary>
    public record CheckResult(
        string CheckId,
        DateTimeOffset ObservedAt,
        CheckStatus Status,
        string? Message = null,
        IDictionary<string, double>? Metrics = null,
        IDictionary<string, string>? Dimensions = null);

    /// <summary>
    /// Attribute used to mark a check implementation with its identifying type.
    /// The scheduler uses this type to resolve the correct ICheck instance from the DI container.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CheckAttribute : Attribute
    {
        public string Type { get; }
        public CheckAttribute(string type) => Type = type;
    }

    /// <summary>
    /// Interface implemented by a check. The scheduler invokes RunAsync at the interval
    /// specified in the descriptor. Implementations should perform their work and
    /// return a CheckResult reflecting success or failure.
    /// </summary>
    public interface ICheck
    {
        Task<CheckResult> RunAsync(CheckDescriptor descriptor, CancellationToken ct);
    }
}