using NUnit.Framework;
using DevMenuTool.Core;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Runtime;

namespace DevMenuTool.Tests.Core
{
    [TestFixture]
    public class DevMenuContainerTests
    {
        [TearDown]
        public void Cleanup()
        {
            DevMenuContainer.Reset();
        }

        [Test]
        public void MethodCache_ShouldReturnMethodCacheInstance()
        {
            var result = DevMenuContainer.MethodCache;

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<MethodCache>());
        }

        [Test]
        public void MethodFactory_ShouldReturnDevMethodFactoryInstance()
        {
            var result = DevMenuContainer.MethodFactory;

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DevMethodFactory>());
        }

        [Test]
        public void MethodScanner_ShouldReturnMethodScannerInstance()
        {
            var result = DevMenuContainer.MethodScanner;

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<MethodScanner>());
        }

        [Test]
        public void MethodInvoker_ShouldReturnMethodInvokerInstance()
        {
            var result = DevMenuContainer.MethodInvoker;

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<MethodInvoker>());
        }

        [Test]
        public void ParameterHandlerRegistry_ShouldReturnParameterHandlerRegistryInstance()
        {
            var result = DevMenuContainer.ParameterHandlerRegistry;

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ParameterHandlerRegistry>());
        }

        [Test]
        public void MethodIndex_ShouldReturnSameInstanceUntilReset()
        {
            var index = DevMenuContainer.MethodIndex;

            Assert.That(index, Is.InstanceOf<DevMenuMethodIndex>());
            Assert.That(DevMenuContainer.MethodIndex, Is.SameAs(index));

            DevMenuContainer.Reset();

            Assert.That(DevMenuContainer.MethodIndex, Is.Not.SameAs(index));
        }

        [Test]
        public void MethodScanner_ShouldUseMethodCacheAndFactory()
        {
            var scanner = DevMenuContainer.MethodScanner;
            var cache = DevMenuContainer.MethodCache;
            var factory = DevMenuContainer.MethodFactory;

            Assert.That(scanner, Is.Not.Null);
            Assert.That(cache, Is.Not.Null);
            Assert.That(factory, Is.Not.Null);
        }

        [Test]
        public void MethodInvoker_ShouldUseParameterHandlerRegistry()
        {
            var invoker = DevMenuContainer.MethodInvoker;
            var registry = DevMenuContainer.ParameterHandlerRegistry;

            Assert.That(invoker, Is.Not.Null);
            Assert.That(registry, Is.Not.Null);
        }

        [Test]
        public void Reset_ShouldClearAllInstances()
        {
            var cache = DevMenuContainer.MethodCache;
            var factory = DevMenuContainer.MethodFactory;
            var scanner = DevMenuContainer.MethodScanner;
            var invoker = DevMenuContainer.MethodInvoker;
            var registry = DevMenuContainer.ParameterHandlerRegistry;

            DevMenuContainer.Reset();

            var newCache = DevMenuContainer.MethodCache;
            var newFactory = DevMenuContainer.MethodFactory;
            var newScanner = DevMenuContainer.MethodScanner;
            var newInvoker = DevMenuContainer.MethodInvoker;
            var newRegistry = DevMenuContainer.ParameterHandlerRegistry;

            Assert.That(newCache, Is.Not.SameAs(cache));
            Assert.That(newFactory, Is.Not.SameAs(factory));
            Assert.That(newScanner, Is.Not.SameAs(scanner));
            Assert.That(newInvoker, Is.Not.SameAs(invoker));
            Assert.That(newRegistry, Is.Not.SameAs(registry));
        }

        [Test]
        public void MultipleCalls_ShouldReturnSameInstances()
        {
            var cache1 = DevMenuContainer.MethodCache;
            var cache2 = DevMenuContainer.MethodCache;

            var factory1 = DevMenuContainer.MethodFactory;
            var factory2 = DevMenuContainer.MethodFactory;

            var scanner1 = DevMenuContainer.MethodScanner;
            var scanner2 = DevMenuContainer.MethodScanner;

            var invoker1 = DevMenuContainer.MethodInvoker;
            var invoker2 = DevMenuContainer.MethodInvoker;

            var registry1 = DevMenuContainer.ParameterHandlerRegistry;
            var registry2 = DevMenuContainer.ParameterHandlerRegistry;

            Assert.That(cache1, Is.SameAs(cache2));
            Assert.That(factory1, Is.SameAs(factory2));
            Assert.That(scanner1, Is.SameAs(scanner2));
            Assert.That(invoker1, Is.SameAs(invoker2));
            Assert.That(registry1, Is.SameAs(registry2));
        }
    }
} 