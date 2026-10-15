using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevMenuTool.Core.Interfaces;
using UnityEngine;

namespace DevMenuTool.Core
{
    public class MethodScanner : IMethodScanner
    {
        private readonly IMethodCache _methodCache;
        private readonly IDevMethodFactory _methodFactory;
        private readonly IDevMenuMethodIndex _methodIndex;

        public MethodScanner(IMethodCache methodCache, IDevMethodFactory methodFactory, IDevMenuMethodIndex methodIndex)
        {
            _methodCache = methodCache ?? throw new ArgumentNullException(nameof(methodCache));
            _methodFactory = methodFactory ?? throw new ArgumentNullException(nameof(methodFactory));
            _methodIndex = methodIndex ?? throw new ArgumentNullException(nameof(methodIndex));
        }

        public IReadOnlyList<IDevMethod> CachedMethods => _methodCache.CachedMethods;

        /// <summary>
        /// Finds live instances of the indexed command types only, instead of reflecting over every
        /// MonoBehaviour, so the cost scales with the number of command-bearing types.
        /// </summary>
        public void ScanScene()
        {
            _methodCache.Clear();
            _methodIndex.EnsureBuilt();

            var indexed = _methodIndex.Methods;
            var methodsByType = new Dictionary<Type, List<IndexedMethod>>();
            var visited = new HashSet<MonoBehaviour>();

            foreach (var type in ScannableTypes(indexed))
            {
                foreach (var obj in Resources.FindObjectsOfTypeAll(type))
                {
                    if (obj is MonoBehaviour mono && mono != null && visited.Add(mono) && IsRuntimeObject(mono.gameObject))
                        AddMethods(mono, MethodsFor(mono.GetType(), indexed, methodsByType));
                }
            }
        }

        public void RebuildIndex()
        {
            _methodIndex.BuildFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            ScanScene();
        }

        private void AddMethods(MonoBehaviour mono, List<IndexedMethod> methods)
        {
            foreach (var entry in methods)
                _methodCache.AddMethod(_methodFactory.CreateDevMethod(entry.Method, mono, entry.Attribute));
        }

        /// <summary>
        /// Declaring types to look up. Resources.FindObjectsOfTypeAll also returns subclass instances and finds
        /// inactive, DontDestroyOnLoad and HideFlags-hidden objects, which FindObjectsByType skips.
        /// Open generic types cannot be looked up and are skipped.
        /// </summary>
        private static IEnumerable<Type> ScannableTypes(IReadOnlyList<IndexedMethod> indexed)
        {
            return indexed
                .Select(entry => entry.DeclaringType)
                .Where(type => typeof(MonoBehaviour).IsAssignableFrom(type) && !type.ContainsGenericParameters)
                .Distinct();
        }

        /// <summary>
        /// Indexed methods that apply to an instance of <paramref name="instanceType"/>. When a base method and
        /// its override both carry [DevMenu], only the most derived one is kept, as reflecting on the instance did.
        /// </summary>
        private static List<IndexedMethod> MethodsFor(Type instanceType, IReadOnlyList<IndexedMethod> indexed,
            Dictionary<Type, List<IndexedMethod>> cache)
        {
            if (cache.TryGetValue(instanceType, out var methods))
                return methods;

            methods = indexed
                .Where(entry => entry.DeclaringType.IsAssignableFrom(instanceType))
                .GroupBy(entry => BaseDefinitionKey(entry.Method))
                .Select(group => group.OrderByDescending(entry => InheritanceDepth(entry.DeclaringType)).First())
                .ToList();

            cache[instanceType] = methods;
            return methods;
        }

        private static (Module, int) BaseDefinitionKey(MethodInfo method)
        {
            var baseDefinition = method.GetBaseDefinition();
            return (baseDefinition.Module, baseDefinition.MetadataToken);
        }

        private static int InheritanceDepth(Type type)
        {
            var depth = 0;
            for (var current = type; current != null; current = current.BaseType)
                depth++;
            return depth;
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
