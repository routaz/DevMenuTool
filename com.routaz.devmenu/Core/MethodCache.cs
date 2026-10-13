using System.Collections.Generic;
using DevMenuTool.Core.Interfaces;

namespace DevMenuTool.Core
{
    public class MethodCache : IMethodCache
    {
        private readonly List<IDevMethod> _cachedMethods = new();

        public IReadOnlyList<IDevMethod> CachedMethods => _cachedMethods.AsReadOnly();

        public void AddMethod(IDevMethod method)
        {
            if (method != null && !_cachedMethods.Contains(method))
            {
                _cachedMethods.Add(method);
            }
        }

        public void Clear()
        {
            _cachedMethods.Clear();
        }

        public void Refresh()
        {
            Clear();
        }
    }
} 