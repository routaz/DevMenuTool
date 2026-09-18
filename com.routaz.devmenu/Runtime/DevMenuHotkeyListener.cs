using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DevMenuTool.Core;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Runtime.Persistence;

namespace DevMenuTool.Runtime
{
    public class DevMenuHotkeyListener : MonoBehaviour
    {
        private IMethodInvoker _methodInvoker;
        private IMethodScanner _methodScanner;
        
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
            _methodScanner.ScanScene();
            RebuildKeyCache();
        }

        private void EnsureScanned()
        {
            if (_methodScanner.CachedMethods.Count == 0)
                _methodScanner.ScanScene();
        }

        private void RebuildKeyCache()
        {
            _keyCache.Clear();
            foreach (var method in _methodScanner.CachedMethods)
            {
                var signature = method.MethodInfo.ToString();
                var key = DevMenuKeybinds.GetKey(signature, method.ShortcutKey);
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
