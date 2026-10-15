using UnityEngine;

namespace DevMenuTool.Runtime
{
    public static class DevMenuPlayModeInjector
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string RuntimeObjectName = "__DevMenuHotkeyListener";
        private const string OverlayObjectName = "__DevMenuOverlay";

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
            go.hideFlags = HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(go);

            if (GameObject.Find(OverlayObjectName) == null)
            {
                var overlay = new GameObject(OverlayObjectName);
                overlay.AddComponent<DevMenuTool.UI.DevMenuOverlay>();
                Object.DontDestroyOnLoad(overlay);
            }
        }
#endif
    }
}