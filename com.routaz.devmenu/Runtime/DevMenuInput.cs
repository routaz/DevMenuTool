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
        private const KeyCode FallbackKey = KeyCode.F1;
        private static bool _warnedUnsupportedKey;
        private static bool _hasCachedKey;
        private static KeyCode _cachedKeyCode;
        private static UnityEngine.InputSystem.Key _cachedKey;
#endif

        /// <summary>
        /// The key that <see cref="WasPressedThisFrame"/> actually reads for <paramref name="requested"/>:
        /// with the Input System, keys <see cref="DevMenuKeyMap"/> doesn't support fall back to F1;
        /// with the legacy Input Manager every key is read as requested.
        /// </summary>
        public static KeyCode EffectiveToggleKey(KeyCode requested)
        {
#if DEVMENU_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
            return DevMenuKeyMap.TryGetInputSystemKeyName(requested, out _) ? requested : FallbackKey;
#else
            return requested;
#endif
        }

        public static bool WasPressedThisFrame(KeyCode keyCode)
        {
#if DEVMENU_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return false;

            if (!_hasCachedKey || _cachedKeyCode != keyCode)
            {
                _cachedKey = ResolveInputSystemKey(keyCode);
                _cachedKeyCode = keyCode;
                _hasCachedKey = true;
            }

            return keyboard[_cachedKey].wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(keyCode);
#else
            return false;
#endif
        }

#if DEVMENU_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
        private static UnityEngine.InputSystem.Key ResolveInputSystemKey(KeyCode keyCode)
        {
            var effective = EffectiveToggleKey(keyCode);
            if (effective != keyCode && !_warnedUnsupportedKey)
            {
                Debug.LogWarning($"[DevMenu] {keyCode} can't be read through the Input System; using {effective}.");
                _warnedUnsupportedKey = true;
            }

            DevMenuKeyMap.TryGetInputSystemKeyName(effective, out var keyName);
            return (UnityEngine.InputSystem.Key)System.Enum.Parse(typeof(UnityEngine.InputSystem.Key), keyName);
        }
#endif

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
