using System;
using System.Reflection;
using DevMenuTool.Core.Attributes;

namespace DevMenuTool.Core
{
    /// <summary>
    /// One <see cref="DevMenuAttribute"/> method found by the <see cref="IDevMenuMethodIndex"/>.
    /// </summary>
    public sealed class IndexedMethod
    {
        public MethodInfo Method { get; }
        public Type DeclaringType { get; }
        public DevMenuAttribute Attribute { get; }

        public IndexedMethod(MethodInfo method, DevMenuAttribute attribute)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            DeclaringType = method.DeclaringType;
        }
    }
}
