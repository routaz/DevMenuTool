using UnityEngine;

namespace DevMenuTool.Runtime.Persistence
{
    /// <summary>
    /// Key/value store backing DevMenu persistence.
    /// Uses EditorPrefs in the editor and PlayerPrefs in player builds.
    /// </summary>
    internal static class DevMenuPrefs
    {
        public static string GetString(string key, string defaultValue)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorPrefs.GetString(key, defaultValue);
#else
            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : defaultValue;
#endif
        }

        public static void SetString(string key, string value)
        {
#if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetString(key, value);
#else
            PlayerPrefs.SetString(key, value);
#endif
        }

        public static void DeleteKey(string key)
        {
#if UNITY_EDITOR
            UnityEditor.EditorPrefs.DeleteKey(key);
#else
            PlayerPrefs.DeleteKey(key);
#endif
        }
    }
}
