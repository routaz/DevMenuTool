using System;
using System.Reflection;
using DevMenuTool.Core.Attributes;
using DevMenuTool.Core.Interfaces;

namespace DevMenuTool.Core
{
    public class DevMethodFactory : IDevMethodFactory
    {
        public IDevMethod CreateDevMethod(MethodInfo methodInfo, object targetInstance, DevMenuAttribute attribute)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));
            
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));
            
            return new DevMethod(methodInfo, targetInstance, attribute);
        }
    }
} 