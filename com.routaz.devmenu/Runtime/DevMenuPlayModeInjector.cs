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
                /*
                Debug.LogWarning($"Found an object named {RuntimeObjectName}. It's preventing a new one from being created.");
                Debug.Log($"Object found: {existingObject.name}");
                Debug.Log($"Active in hierarchy: {existingObject.activeInHierarchy}");
                Debug.Log($"Object has DevMenuHotkeyListener component: {existingObject.GetComponent<DevMenuHotkeyListener>() != null}");
                */
                Object.DestroyImmediate(existingObject);
            }

            var go = new GameObject(RuntimeObjectName);
            go.AddComponent<DevMenuHotkeyListener>();
            go.hideFlags = HideFlags.HideAndDontSave; // Invisible in hierarchy
            Object.DontDestroyOnLoad(go);
        }
    }
}