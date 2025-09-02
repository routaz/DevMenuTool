using System.Reflection;
using UnityEngine;

namespace DevMenuTool.Core.Interfaces
{
    public interface IDevMethodInfo
    {
        string GroupName { get; }
        string ButtonLabel { get; }
        KeyCode ShortcutKey { get; }
        MethodInfo MethodInfo { get; }
        object TargetInstance { get; }
        ParameterInfo[] Parameters { get; }
    }
} 