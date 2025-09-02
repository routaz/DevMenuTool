using System.Collections.Generic;
using DevMenuTool.Core.Interfaces;

namespace DevMenuTool.Core
{
    public interface IMethodScanner
    {
        IReadOnlyList<IDevMethod> CachedMethods { get; }
        void ScanScene();
        void ClearCache();
    }
} 