using UnityEngine;
using DevMenuTool.Core.Interfaces;

namespace DevMenuTool.Core
{
    public class DevMenuConfiguration : IDevMenuConfiguration
    {
        public KeyCode DefaultShortcutKey => KeyCode.None;
        public string DefaultGroupName => "Default";
        public bool EnableHotkeys => true;
        public bool EnableParameterPersistence => true;
        public bool EnableMethodGrouping => true;
    }
} 