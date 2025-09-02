using UnityEditor;
using UnityEngine;

namespace DevMenuTool.Editor
{
    public static class DevMenuKeybinds
    {
        private static string GetPrefKey(string methodSignature) => $"DevMenuKey_{methodSignature}";

        public static KeyCode GetKey(string methodSignature, KeyCode defaultKey)
        {
            string keyName = EditorPrefs.GetString(GetPrefKey(methodSignature), defaultKey.ToString());

            if (System.Enum.TryParse(keyName, out KeyCode key))
                return key;

            return defaultKey;
        }

        public static void SetKey(string methodSignature, KeyCode key)
        {
            EditorPrefs.SetString(GetPrefKey(methodSignature), key.ToString());
        }
    }
}