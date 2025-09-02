using UnityEngine;
using DevMenuTool.Core;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Runtime;

#if UNITY_EDITOR
using DevMenuTool.Editor;
#endif

namespace DevMenuTool.Runtime
{
    public class DevMenuHotkeyListener : MonoBehaviour
    {
        private IMethodInvoker _methodInvoker;
        private IMethodScanner _methodScanner;

        private void Awake()
        {
            _methodInvoker = DevMenuContainer.MethodInvoker;
            _methodScanner = DevMenuContainer.MethodScanner;
        }

        private void Update()
        {
            foreach (var method in _methodScanner.CachedMethods)
            {
                KeyCode key = method.ShortcutKey;

#if UNITY_EDITOR
                key = DevMenuKeybinds.GetKey(method.MethodInfo.ToString(), method.ShortcutKey);
#endif

                if (Input.GetKeyDown(key))
                {
                    _methodInvoker.Invoke(method);
                }
            }
        }
    }
}