using System;

namespace DevMenuTool.Runtime.Interfaces
{
    public interface IParameterHandler
    {
        bool CanHandle(Type parameterType);
        object Parse(string input, Type targetType);
        string ToString(object value);
    }
} 