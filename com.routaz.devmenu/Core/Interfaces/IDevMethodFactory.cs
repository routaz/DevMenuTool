using System.Reflection;
using DevMenuTool.Core.Attributes;

namespace DevMenuTool.Core.Interfaces
{
    public interface IDevMethodFactory
    {
        IDevMethod CreateDevMethod(MethodInfo methodInfo, object targetInstance, DevMenuAttribute attribute);
    }
} 