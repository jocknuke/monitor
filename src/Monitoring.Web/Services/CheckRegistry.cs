using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Monitoring.Web.Contracts;

namespace Monitoring.Web.Services
{
    /// <summary>
    /// Resolves ICheck implementations by their CheckAttribute type. At startup,
    /// the registry collects all registered ICheck instances and indexes them
    /// by the type string specified on their CheckAttribute.
    /// </summary>
    public interface ICheckRegistry
    {
        /// <summary>
        /// Returns the ICheck implementation for the given type, or null if none exists.
        /// </summary>
        ICheck? GetCheck(string type);
    }

    public class CheckRegistry : ICheckRegistry
    {
        private readonly IDictionary<string, ICheck> _checks;

        public CheckRegistry(IEnumerable<ICheck> checks)
        {
            _checks = new Dictionary<string, ICheck>(StringComparer.OrdinalIgnoreCase);
            foreach (var check in checks)
            {
                var attr = check.GetType().GetCustomAttribute<CheckAttribute>();
                if (attr != null && !_checks.ContainsKey(attr.Type))
                {
                    _checks[attr.Type] = check;
                }
            }
        }

        public ICheck? GetCheck(string type)
        {
            return _checks.TryGetValue(type, out var check) ? check : null;
        }
    }
}