using System;
using DevMenuTool.Runtime.Interfaces;

namespace DevMenuTool.Runtime.Handlers
{
    public class BooleanParameterHandler : IParameterHandler
    {
        public bool CanHandle(Type parameterType)
        {
            return parameterType == typeof(bool);
        }

        public object Parse(string input, Type targetType)
        {
            return bool.Parse(input.ToLower());
        }

        public string ToString(object value)
        {
            return value?.ToString() ?? string.Empty;
        }
    }
} 