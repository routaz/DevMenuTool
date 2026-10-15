using UnityEngine;

namespace DevMenuTool.Runtime
{
    /// <summary>
    /// Reads keys and the pointer from the Input System when the project has it, otherwise from the
    /// legacy Input Manager. Returns false when neither is available.
    /// </summary>
    public static class DevMenuInput
    {
#if DEVMENU_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
        private const string FallbackKeyName = "F1";
        private static bool _warnedUnsupportedKey;
#endif

        public static bool WasPressedThisFrame(KeyCode keyCode)
        {
#if DEVMENU_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return false;

            if (!DevMenuKeyMap.TryGetInputSystemKeyName(keyCode, out var keyName))
            {
                if (!_warnedUnsupportedKey)
                {
                    Debug.LogWarning($"[DevMenu] {keyCode} can't be read through the Input System; using {FallbackKeyName}.");
                    _warnedUnsupportedKey = true;
                }
                keyName = FallbackKeyName;
            }

            var key = (UnityEngine.InputSystem.Key)System.Enum.Parse(typeof(UnityEngine.InputSystem.Key), keyName);
            return keyboard[key].wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(keyCode);
#else
            return false;
#endif
        }

        public static bool TryGetPointerPosition(out Vector2 screenPosition)
        {
#if DEVMENU_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null)
            {
                screenPosition = mouse.position.ReadValue();
                return true;
            }
            screenPosition = default;
            return false;
#elif ENABLE_LEGACY_INPUT_MANAGER
            screenPosition = Input.mousePosition;
            return true;
#else
            screenPosition = default;
            return false;
#endif
        }
    }
}
