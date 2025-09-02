using System.Collections.Generic;
using DevMenuTool.Core.Interfaces;

namespace DevMenuTool.Core
{
    public interface IMethodCache
    {
        IReadOnlyList<IDevMethod> CachedMethods { get; }
        void AddMethod(IDevMethod method);
        void Clear();
        void Refresh();
    }
} 