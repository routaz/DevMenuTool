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

            var monoBehaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);

            foreach (var mono in monoBehaviours)
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

        public void ClearCache()
        {
            _methodCache.Clear();
        }
    }
}