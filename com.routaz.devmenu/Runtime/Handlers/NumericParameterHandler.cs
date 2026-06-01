using System;
using System.Globalization;
using DevMenuTool.Runtime.Interfaces;

namespace DevMenuTool.Runtime.Handlers
{
    public class NumericParameterHandler : IParameterHandler
    {
        public bool CanHandle(Type parameterType)
        {
            return parameterType == typeof(int) || parameterType == typeof(float);
        }

        public object Parse(string input, Type targetType)
        {
            if (targetType == typeof(int))
                return int.Parse(input, CultureInfo.InvariantCulture);

            if (targetType == typeof(float))
                return float.Parse(input, CultureInfo.InvariantCulture);

            throw new NotSupportedException($"Unsupported numeric type: {targetType.Name}");
        }

        public string ToString(object value)
        {
            return value is IFormattable formattable
                ? formattable.ToString(null, CultureInfo.InvariantCulture)
                : value?.ToString() ?? string.Empty;
        }
    }
} 