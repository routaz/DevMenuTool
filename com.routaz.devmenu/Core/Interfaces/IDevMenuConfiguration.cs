using UnityEngine;

namespace DevMenuTool.Core.Interfaces
{
    public interface IDevMenuConfiguration
    {
        KeyCode DefaultShortcutKey { get; }
        string DefaultGroupName { get; }
        bool EnableHotkeys { get; }
        bool EnableParameterPersistence { get; }
        bool EnableMethodGrouping { get; }
    }
} 