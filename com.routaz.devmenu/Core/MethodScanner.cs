using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Core.Attributes;
using UnityEngine;

namespace DevMenuTool.Core
{
    public class MethodScanner : IMethodScanner
    {
        private readonly IMethodCache _methodCache;
        private readonly IDevMethodFactory _methodFactory;

        public MethodScanner(IMethodCache methodCache, IDevMethodFactory methodFactory)
        {
            _methodCache = methodCache ?? throw new ArgumentNullException(nameof(methodCache));
            _methodFactory = methodFactory ?? throw new ArgumentNullException(nameof(methodFactory));
        }

        public IReadOnlyList<IDevMethod> CachedMethods => _methodCache.CachedMethods;

        public void ScanScene()
        {
            _methodCache.Clear();

            foreach (var mono in FindSceneMonoBehaviours())
            {
                var methods = mono.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(m => m.GetCustomAttribute<DevMenuAttribute>() != null);

                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute<DevMenuAttribute>();

                    if (attr != null)
                    {
                        var devMethod = _methodFactory.CreateDevMethod(method, mono, attr);
                        _methodCache.AddMethod(devMethod);
                    }
                }
            }
        }

        /// <summary>
        /// Returns MonoBehaviours living in loaded scenes (including DontDestroyOnLoad), inactive ones,
        /// and ones hidden via HideFlags (e.g. HideAndDontSave), which FindObjectsByType skips.
        /// </summary>
        private static IEnumerable<MonoBehaviour> FindSceneMonoBehaviours()
        {
            return Resources.FindObjectsOfTypeAll<MonoBehaviour>()
                .Where(mono => mono != null && IsRuntimeObject(mono.gameObject));
        }

        private static bool IsRuntimeObject(GameObject go)
        {
            if (go.scene.IsValid())
                return go.scene.isLoaded;

            // DontSave objects are not attached to any scene. Prefab/other assets also have
            // no valid scene but never carry the DontSave flag, so they are excluded here.
            return (go.hideFlags & HideFlags.DontSave) != 0;
        }

        public void ClearCache()
        {
            _methodCache.Clear();
        }
    }
}