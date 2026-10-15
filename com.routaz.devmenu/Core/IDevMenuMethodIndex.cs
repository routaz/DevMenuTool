using System;
using System.Collections.Generic;
using System.Reflection;

namespace DevMenuTool.Core
{
    /// <summary>
    /// The list of <c>[DevMenu]</c> instance methods in the loaded code, built once instead of
    /// reflecting over every scene object on each scan.
    /// </summary>
    public interface IDevMenuMethodIndex
    {
        IReadOnlyList<IndexedMethod> Methods { get; }
        bool IsBuilt { get; }

        /// <summary>Raised on the calling thread after <see cref="Set"/> or <see cref="BuildFromAssemblies"/>.</summary>
        event Action Rebuilt;

        void Set(IEnumerable<MethodInfo> methods);
        void BuildFromAssemblies(IEnumerable<Assembly> assemblies);
        void BuildInBackground();
        void EnsureBuilt();
    }
}
