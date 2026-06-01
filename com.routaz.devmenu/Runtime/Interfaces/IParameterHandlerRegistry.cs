using System;

namespace DevMenuTool.Runtime.Interfaces
{
    /// <summary>
    /// Resolves parameter handlers and converts values to and from their string representation.
    /// </summary>
    public interface IParameterHandlerRegistry
    {
        IParameterHandler GetHandler(Type parameterType);
        bool IsSupported(Type type);
        object Parse(string input, Type targetType);
        string ToString(object value);
    }
}
