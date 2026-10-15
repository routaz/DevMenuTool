using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DevMenuTool.Core.Attributes;
using UnityEngine;

namespace DevMenuTool.Core
{
    /// <summary>
    /// Index of <c>[DevMenu]</c> instance methods. The editor fills it from <c>TypeCache</c> on every
    /// domain load; players build it on a worker thread at startup. Static methods are ignored.
    /// The finished list is published by swapping one reference, so readers never see a partial list.
    /// </summary>
    public sealed class DevMenuMethodIndex : IDevMenuMethodIndex
    {
        private const BindingFlags DeclaredInstanceMethods =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        private static readonly Assembly DevMenuAssembly = typeof(DevMenuAttribute).Assembly;

        private IndexedMethod[] _methods;
        private Task _backgroundBuild;

        public IReadOnlyList<IndexedMethod> Methods => Volatile.Read(ref _methods) ?? Array.Empty<IndexedMethod>();
        public bool IsBuilt => Volatile.Read(ref _methods) != null;

        public event Action Rebuilt;

        public void Set(IEnumerable<MethodInfo> methods)
        {
            Volatile.Write(ref _methods, Collect(methods));
            Rebuilt?.Invoke();
        }

        public void BuildFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            Set(FindMethods(assemblies));
        }

        /// <summary>
        /// Builds the index on a worker thread. The result is only published if nothing else built the
        /// index first, and <see cref="Rebuilt"/> is not raised, because it would run on the worker thread.
        /// </summary>
        public void BuildInBackground()
        {
            if (IsBuilt || _backgroundBuild != null)
                return;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            _backgroundBuild = Task.Run(() =>
                Interlocked.CompareExchange(ref _methods, Collect(FindMethods(assemblies)), null));
        }

        public void EnsureBuilt()
        {
            if (IsBuilt)
                return;

            var pending = _backgroundBuild;
            if (pending != null)
            {
                try
                {
                    pending.Wait();
                }
                catch (AggregateException e)
                {
                    Debug.LogException(e.InnerException ?? e);
                }

                if (IsBuilt)
                    return;
            }

            BuildFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <summary>True for the DevMenuTool assembly and for assemblies that reference it.</summary>
        public static bool IsCandidateAssembly(Assembly assembly)
        {
            if (assembly == DevMenuAssembly)
                return true;

            var name = DevMenuAssembly.GetName().Name;
            return assembly.GetReferencedAssemblies().Any(reference => reference.Name == name);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void StartPlayerBuild()
        {
            // In the editor the index is already built from TypeCache, so this is a no-op there.
            DevMenuContainer.MethodIndex.BuildInBackground();
        }

        private static IEnumerable<MethodInfo> FindMethods(IEnumerable<Assembly> assemblies)
        {
            return assemblies
                .Where(assembly => !assembly.IsDynamic && IsCandidateAssembly(assembly))
                .SelectMany(GetLoadableTypes)
                .SelectMany(type => type.GetMethods(DeclaredInstanceMethods));
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(type => type != null);
            }
        }

        private static IndexedMethod[] Collect(IEnumerable<MethodInfo> methods)
        {
            return methods
                .Where(method => method != null && !method.IsStatic)
                .Select(method => (method, attribute: method.GetCustomAttribute<DevMenuAttribute>(false)))
                .Where(pair => pair.attribute != null)
                .Select(pair => new IndexedMethod(pair.method, pair.attribute))
                .ToArray();
        }
    }
}
