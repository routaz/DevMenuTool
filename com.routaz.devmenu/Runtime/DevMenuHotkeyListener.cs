using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DevMenuTool.Core;
using DevMenuTool.Core.Interfaces;

#if UNITY_EDITOR
using DevMenuTool.Editor;
#endif

namespace DevMenuTool.Runtime
{
    public class DevMenuHotkeyListener : MonoBehaviour
    {
        private IMethodInvoker _methodInvoker;
        private IMethodScanner _methodScanner;

        // Resolved key per method signature, cached so we don't read EditorPrefs every frame.
        private readonly Dictionary<string, KeyCode> _keyCache = new();

        private void Awake()
        {
            _methodInvoker = DevMenuContainer.MethodInvoker;
            _methodScanner = DevMenuContainer.MethodScanner;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureScanned();
            RebuildKeyCache();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // A new scene brings new MonoBehaviours, so force a fresh scan.
            _methodScanner.ScanScene();
            RebuildKeyCache();
        }

        private void EnsureScanned()
        {
            // The scanner cache is shared (e.g. with the runtime UI). Only scan if no one has yet,
            // so the listener and UI operate on the same DevMethod instances.
            if (_methodScanner.CachedMethods.Count == 0)
                _methodScanner.ScanScene();
        }

        private void RebuildKeyCache()
        {
            _keyCache.Clear();
            foreach (var method in _methodScanner.CachedMethods)
            {
                var signature = method.MethodInfo.ToString();
                var key = method.ShortcutKey;
#if UNITY_EDITOR
                key = DevMenuKeybinds.GetKey(signature, method.ShortcutKey);
#endif
                _keyCache[signature] = key;
            }
        }

        private void Update()
        {
            var methods = _methodScanner.CachedMethods;
            for (int i = 0; i < methods.Count; i++)
            {
                var method = methods[i];
                if (!_keyCache.TryGetValue(method.MethodInfo.ToString(), out var key))
                    key = method.ShortcutKey;

                if (key != KeyCode.None && Input.GetKeyDown(key))
                    _methodInvoker.Invoke(method);
            }
        }
    }
}
