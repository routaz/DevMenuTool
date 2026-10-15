using UnityEngine;

namespace DevMenuTool.Runtime
{
    /// <summary>
    /// Maps the keys the overlay can be toggled with to Input System <c>Key</c> enum names.
    /// Kept free of any Input System types so it compiles and tests without the package.
    /// </summary>
    public static class DevMenuKeyMap
    {
        public static bool TryGetInputSystemKeyName(KeyCode keyCode, out string keyName)
        {
            if (keyCode >= KeyCode.F1 && keyCode <= KeyCode.F12)
            {
                keyName = keyCode.ToString();
                return true;
            }

            if (keyCode == KeyCode.BackQuote)
            {
                keyName = "Backquote";
                return true;
            }

            keyName = null;
            return false;
        }
    }
}
