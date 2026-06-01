using System;
using System.Collections.Generic;
using System.Linq;
using DevMenuTool.Runtime.Interfaces;
using DevMenuTool.Runtime.Handlers;

namespace DevMenuTool.Runtime
{
    public class ParameterHandlerRegistry : IParameterHandlerRegistry
    {
        private readonly List<IParameterHandler> _handlers;

        public ParameterHandlerRegistry()
        {
            _handlers = new List<IParameterHandler>
            {
                new StringParameterHandler(),
                new NumericParameterHandler(),
                new BooleanParameterHandler(),
                new EnumParameterHandler()
            };
        }

        public IParameterHandler GetHandler(Type parameterType)
        {
            return _handlers.FirstOrDefault(h => h.CanHandle(parameterType));
        }

        public bool IsSupported(Type type)
        {
            return _handlers.Any(h => h.CanHandle(type));
        }

        public object Parse(string input, Type targetType)
        {
            var handler = GetHandler(targetType);
            if (handler == null)
            {
                throw new NotSupportedException($"Unsupported parameter type: {targetType.Name}");
            }

            return handler.Parse(input, targetType);
        }

        public string ToString(object value)
        {
            if (value == null) return string.Empty;

            var handler = GetHandler(value.GetType());
            return handler?.ToString(value) ?? value.ToString();
        }
    }
} 