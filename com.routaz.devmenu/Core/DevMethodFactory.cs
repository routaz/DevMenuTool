using System;
using System.Reflection;
using DevMenuTool.Core.Attributes;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Runtime;
using DevMenuTool.Runtime.Interfaces;

namespace DevMenuTool.Core
{
    public class DevMethodFactory : IDevMethodFactory
    {
        private readonly IParameterHandlerRegistry _registry;

        public DevMethodFactory() : this(new ParameterHandlerRegistry()) { }

        public DevMethodFactory(IParameterHandlerRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public IDevMethod CreateDevMethod(MethodInfo methodInfo, object targetInstance, DevMenuAttribute attribute)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            var parameters = methodInfo.GetParameters();
            var methodKey = methodInfo.ToString();

            var parameterManager = new ParameterManager(parameters, methodKey, _registry);
            var parameterValidator = new ParameterValidator(parameters);
            var eventManager = new ParameterEventManager(parameters, methodInfo.Name);

            return new DevMethod(methodInfo, targetInstance, attribute, parameterManager, parameterValidator, eventManager);
        }
    }
}
