#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DevMenuTool.Editor
{
    public static class DevMenuParameterPrefs
    {
        private static readonly HashSet<string> _savedKeys = new HashSet<string>();
        private const string KeysRegistryKey = "DevMenuParam_KeysRegistry";
        private const string TestKeysRegistryKey = "DevMenuParam_TEST_KeysRegistry";
        private static bool _isTestMode = false;

        private static string GetPrefKey(string methodSignature, string paramName) 
        {
            var prefix = _isTestMode ? "TEST_" : "";
            return $"{prefix}DevMenuParam_{methodSignature}_{paramName}";
        }

        public static void SetValue(string methodSignature, string paramName, object value)
        {
            if (value == null) return;
            var key = GetPrefKey(methodSignature, paramName);
            EditorPrefs.SetString(key, value.ToString());
            _savedKeys.Add(key);
            SaveKeysRegistry();
        }

        public static string GetValue(string methodSignature, string paramName)
        {
            return EditorPrefs.GetString(GetPrefKey(methodSignature, paramName), null);
        }

        public static void ClearAll()
        {
            LoadKeysRegistry();
            foreach (var key in _savedKeys)
            {
                EditorPrefs.DeleteKey(key);
            }
            _savedKeys.Clear();
            var registryKey = _isTestMode ? TestKeysRegistryKey : KeysRegistryKey;
            EditorPrefs.DeleteKey(registryKey);
        }

        public static void SetTestMode(bool isTestMode)
        {
            _isTestMode = isTestMode;
            _savedKeys.Clear(); // Clear the in-memory registry when switching modes
        }

        private static void SaveKeysRegistry()
        {
            var registryKey = _isTestMode ? TestKeysRegistryKey : KeysRegistryKey;
            var keysJson = JsonUtility.ToJson(new KeysData { keys = _savedKeys.ToArray() });
            EditorPrefs.SetString(registryKey, keysJson);
        }

        private static void LoadKeysRegistry()
        {
            var registryKey = _isTestMode ? TestKeysRegistryKey : KeysRegistryKey;
            var keysJson = EditorPrefs.GetString(registryKey, "");
            if (!string.IsNullOrEmpty(keysJson))
            {
                try
                {
                    var data = JsonUtility.FromJson<KeysData>(keysJson);
                    _savedKeys.Clear();
                    if (data?.keys != null)
                    {
                        foreach (var key in data.keys)
                        {
                            _savedKeys.Add(key);
                        }
                    }
                }
                catch
                {
                    _savedKeys.Clear();
                }
            }
        }

        [System.Serializable]
        private class KeysData
        {
            public string[] keys;
        }
    }
}
#endif