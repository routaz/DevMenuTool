using System;
using DevMenuTool.Runtime.Interfaces;

namespace DevMenuTool.Runtime.Handlers
{
    public class EnumParameterHandler : IParameterHandler
    {
        public bool CanHandle(Type parameterType)
        {
            return parameterType.IsEnum;
        }

        public object Parse(string input, Type targetType)
        {
            return Enum.Parse(targetType, input, ignoreCase: true);
        }

        public string ToString(object value)
        {
            return value?.ToString() ?? string.Empty;
        }
    }
} 