using System;
using System.Collections.Generic;
using System.Linq;
using DevMenuTool.Core.Interfaces;

namespace DevMenuTool.UI
{
    public sealed class DevMenuCommandGroup
    {
        public string Name { get; }
        public IReadOnlyList<IDevMethod> Methods { get; }

        public DevMenuCommandGroup(string name, IReadOnlyList<IDevMethod> methods)
        {
            Name = name;
            Methods = methods;
        }
    }

    /// <summary>Groups commands by <see cref="IDevMethodInfo.GroupName"/>; groups and their commands are sorted by name.</summary>
    public static class DevMenuCommandGroups
    {
        public const string DefaultGroupName = "General";

        public static IReadOnlyList<DevMenuCommandGroup> Build(IEnumerable<IDevMethod> methods)
        {
            return methods
                .GroupBy(m => string.IsNullOrEmpty(m.GroupName) ? DefaultGroupName : m.GroupName)
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .Select(g => new DevMenuCommandGroup(
                    g.Key,
                    g.OrderBy(m => m.ButtonLabel, StringComparer.OrdinalIgnoreCase).ToList()))
                .ToList();
        }
    }
}
