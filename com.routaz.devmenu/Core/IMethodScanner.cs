using System.Collections.Generic;
using DevMenuTool.Core.Interfaces;

namespace DevMenuTool.Core
{
    public interface IMethodScanner
    {
        IReadOnlyList<IDevMethod> CachedMethods { get; }
        void ScanScene();
        void ClearCache();

        /// <summary>Rebuilds the [DevMenu] method index from the loaded assemblies, then rescans.</summary>
        void RebuildIndex();
    }
} 