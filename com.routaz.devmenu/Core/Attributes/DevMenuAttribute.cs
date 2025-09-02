using System;
using UnityEngine;

namespace DevMenuTool.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DevMenuAttribute : Attribute
    {
        public KeyCode ShortcutKey { get; }
        public string GroupName { get; }

        public DevMenuAttribute(KeyCode shortcutKey, string groupName)
        {
            ShortcutKey = shortcutKey;
            GroupName = groupName;
        }
    }
}