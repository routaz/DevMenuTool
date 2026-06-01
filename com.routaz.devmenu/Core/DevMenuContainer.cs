using DevMenuTool.Core.Interfaces;
using DevMenuTool.Runtime;
using DevMenuTool.Runtime.Interfaces;

namespace DevMenuTool.Core
{
    public static class DevMenuContainer
    {
        private static IMethodCache _methodCache;
        private static IDevMethodFactory _methodFactory;
        private static IMethodScanner _methodScanner;
        private static IMethodInvoker _methodInvoker;
        private static IParameterHandlerRegistry _parameterHandlerRegistry;

        public static IMethodCache MethodCache => _methodCache ??= new MethodCache();
        public static IDevMethodFactory MethodFactory => _methodFactory ??= new DevMethodFactory(ParameterHandlerRegistry);
        public static IMethodScanner MethodScanner => _methodScanner ??= new MethodScanner(MethodCache, MethodFactory);
        public static IMethodInvoker MethodInvoker => _methodInvoker ??= new MethodInvoker();
        public static IParameterHandlerRegistry ParameterHandlerRegistry => _parameterHandlerRegistry ??= new ParameterHandlerRegistry();

        public static void Reset()
        {
            _methodCache = null;
            _methodFactory = null;
            _methodScanner = null;
            _methodInvoker = null;
            _parameterHandlerRegistry = null;
        }
    }
} 