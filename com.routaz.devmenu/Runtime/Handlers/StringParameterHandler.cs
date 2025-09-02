using System;
using DevMenuTool.Runtime.Interfaces;

namespace DevMenuTool.Runtime.Handlers
{
    public class StringParameterHandler : IParameterHandler
    {
        public bool CanHandle(Type parameterType)
        {
            return parameterType == typeof(string);
        }

        public object Parse(string input, Type targetType)
        {
            return input;
        }

        public string ToString(object value)
        {
            return value?.ToString() ?? string.Empty;
        }
    }
} 