using System;
using UnityEngine;

namespace DevMenuTool.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DevMenuAttribute : Attribute
    {
        /// <summary>
        /// Hotkey that invokes the method in Play Mode. <see cref="KeyCode.None"/> means
        /// "menu-only": the method is listed in the menu but has no hotkey.
        /// </summary>
        public KeyCode ShortcutKey { get; }
        public string GroupName { get; }

        public DevMenuAttribute(KeyCode shortcutKey, string groupName)
        {
            ShortcutKey = shortcutKey;
            GroupName = groupName;
        }

        /// <summary>
        /// Menu-only entry without a hotkey (equivalent to passing <see cref="KeyCode.None"/>).
        /// </summary>
        public DevMenuAttribute(string groupName) : this(KeyCode.None, groupName)
        {
        }
    }
}
