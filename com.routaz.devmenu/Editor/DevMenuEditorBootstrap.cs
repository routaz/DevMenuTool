using System.Linq;
using UnityEditor;
using UnityEngine;
using DevMenuTool.Core;
using DevMenuTool.Core.Attributes;

namespace DevMenuTool.Editor
{
    /// <summary>
    /// Fills the [DevMenu] method index from TypeCache on every domain load (project open and each
    /// recompile), so the editor never reflects over assemblies or scene objects to find commands.
    /// </summary>
    [InitializeOnLoad]
    public static class DevMenuEditorBootstrap
    {
        static DevMenuEditorBootstrap()
        {
            BuildIndexFromTypeCache();
        }

        [MenuItem("DevMenu/Rescan Commands")]
        public static void RescanCommands()
        {
            BuildIndexFromTypeCache();
            DevMenuContainer.MethodScanner.ScanScene();

            var methods = DevMenuContainer.MethodIndex.Methods;
            var typeCount = methods.Select(m => m.DeclaringType).Distinct().Count();
            Debug.Log($"[DevMenu] Rescanned: {methods.Count} commands on {typeCount} types.");
        }

        private static void BuildIndexFromTypeCache()
        {
            DevMenuContainer.MethodIndex.Set(TypeCache.GetMethodsWithAttribute<DevMenuAttribute>());
        }
    }
}
