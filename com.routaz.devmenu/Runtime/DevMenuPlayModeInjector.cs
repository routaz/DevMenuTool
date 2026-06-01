using UnityEngine;

namespace DevMenuTool.Runtime
{
    public static class DevMenuPlayModeInjector
    {
        private const string RuntimeObjectName = "__DevMenuHotkeyListener";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnPlayModeStart()
        {
            Debug.Log("✅ DevMenuPlayModeInjector initialized.");
            
            var existingObject = GameObject.Find(RuntimeObjectName);

            if (existingObject != null)
            {
                Object.DestroyImmediate(existingObject);
            }

            var go = new GameObject(RuntimeObjectName);
            go.AddComponent<DevMenuHotkeyListener>();
            go.hideFlags = HideFlags.HideAndDontSave; // Invisible in hierarchy
            Object.DontDestroyOnLoad(go);
        }
    }
}