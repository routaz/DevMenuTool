using System.Reflection;
using NUnit.Framework;
using DevMenuTool.Core;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Core.Attributes;
using UnityEngine;

namespace DevMenuTool.Tests.Core
{
    [TestFixture]
    public class DevMethodFactoryTests
    {
        private IDevMethodFactory _factory;
        private TestClass _testInstance;

        [SetUp]
        public void Setup()
        {
            _factory = new DevMethodFactory();
            _testInstance = new TestClass();
        }

        [Test]
        public void CreateDevMethod_WithValidParameters_ShouldCreateDevMethod()
        {
            var methodInfo = typeof(TestClass).GetMethod("TestMethod");
            var attribute = new DevMenuAttribute(KeyCode.T, "TestGroup");

            var result = _factory.CreateDevMethod(methodInfo, _testInstance, attribute);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.MethodInfo, Is.EqualTo(methodInfo));
            Assert.That(result.TargetInstance, Is.EqualTo(_testInstance));
            Assert.That(result.GroupName, Is.EqualTo("TestGroup"));
            Assert.That(result.ShortcutKey, Is.EqualTo(KeyCode.T));
            Assert.That(result.ButtonLabel, Is.EqualTo("TestMethod"));
        }

        [Test]
        public void CreateDevMethod_WithNullMethodInfo_ShouldThrowException()
        {
            var attribute = new DevMenuAttribute(KeyCode.T, "TestGroup");

            Assert.Throws<System.ArgumentNullException>(() => 
                _factory.CreateDevMethod(null, _testInstance, attribute));
        }

        [Test]
        public void CreateDevMethod_WithNullTargetInstance_ShouldCreateDevMethod()
        {
            var methodInfo = typeof(TestClass).GetMethod("TestMethod");
            var attribute = new DevMenuAttribute(KeyCode.T, "TestGroup");

            var result = _factory.CreateDevMethod(methodInfo, null, attribute);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.TargetInstance, Is.Null);
        }

        [Test]
        public void CreateDevMethod_WithNullAttribute_ShouldThrowException()
        {
            var methodInfo = typeof(TestClass).GetMethod("TestMethod");

            Assert.Throws<System.ArgumentNullException>(() => 
                _factory.CreateDevMethod(methodInfo, _testInstance, null));
        }

        [Test]
        public void CreateDevMethod_WithMethodWithParameters_ShouldCreateDevMethodWithParameters()
        {
            var methodInfo = typeof(TestClass).GetMethod("TestMethodWithParams");
            var attribute = new DevMenuAttribute(KeyCode.T, "TestGroup");

            var result = _factory.CreateDevMethod(methodInfo, _testInstance, attribute);

            Assert.That(result.Parameters.Length, Is.EqualTo(2));
            Assert.That(result.Parameters[0].Name, Is.EqualTo("param1"));
            Assert.That(result.Parameters[0].ParameterType, Is.EqualTo(typeof(int)));
            Assert.That(result.Parameters[1].Name, Is.EqualTo("param2"));
            Assert.That(result.Parameters[1].ParameterType, Is.EqualTo(typeof(string)));
        }

        [Test]
        public void CreateDevMethod_WithMethodWithDefaultParameters_ShouldCreateDevMethodWithDefaults()
        {
            var methodInfo = typeof(TestClass).GetMethod("TestMethodWithDefaults");
            var attribute = new DevMenuAttribute(KeyCode.T, "TestGroup");

            var result = _factory.CreateDevMethod(methodInfo, _testInstance, attribute);

            Assert.That(result.Parameters.Length, Is.EqualTo(2));
            Assert.That(result.Parameters[0].HasDefaultValue, Is.True);
            Assert.That(result.Parameters[0].DefaultValue, Is.EqualTo(42));
            Assert.That(result.Parameters[1].HasDefaultValue, Is.True);
            Assert.That(result.Parameters[1].DefaultValue, Is.EqualTo("default"));
        }

        public class TestClass
        {
            public void TestMethod()
            {
            }

            public void TestMethodWithParams(int param1, string param2)
            {
            }

            public void TestMethodWithDefaults(int param1 = 42, string param2 = "default")
            {
            }
        }
    }
} 