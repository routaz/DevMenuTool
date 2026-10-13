using System;
using System.Reflection;
using NUnit.Framework;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Core;
using DevMenuTool.Core.Attributes;
using UnityEngine;

#if UNITY_EDITOR
using DevMenuTool.Runtime.Persistence;
#endif

namespace DevMenuTool.Tests
{
    [TestFixture]
    public class EnhancedDevMethodTests
    {
        private IDevMethod _devMethod;
        private TestClass _testInstance;

        [SetUp]
        public void SetUp()
        {
            _testInstance = new TestClass();
            var methodInfo = typeof(TestClass).GetMethod("TestMethod");
            var attribute = new DevMenuAttribute(KeyCode.G, "Test Group");
            _devMethod = new DevMethodFactory().CreateDevMethod(methodInfo, _testInstance, attribute);
            
            #if UNITY_EDITOR
            DevMenuParameterPrefs.SetTestMode(true);
            DevMenuParameterPrefs.ClearAll();
            #endif
            
            _devMethod.ResetToDefaults();
        }

        [TearDown]
        public void TearDown()
        {
            #if UNITY_EDITOR
            DevMenuParameterPrefs.ClearAll();
            DevMenuParameterPrefs.SetTestMode(false);
            #endif
        }

        [Test]
        public void GetCurrentParameters_ShouldReturnDefaultValues_WhenNoParametersSet()
        {
            var parameters = _devMethod.GetCurrentParameters();

            Assert.That(parameters.Length, Is.EqualTo(2));
            Assert.That(parameters[0], Is.EqualTo(100));
            Assert.That(parameters[1], Is.EqualTo("default"));
        }

        [Test]
        public void SetParameter_ShouldUpdateParameterValue()
        {
            var newValue = 500;

            _devMethod.SetParameter(0, newValue);
            var parameters = _devMethod.GetCurrentParameters();

            Assert.That(parameters[0], Is.EqualTo(newValue));
        }

        [Test]
        public void ResetToDefaults_ShouldRestoreDefaultValues()
        {
            _devMethod.SetParameter(0, 999);
            _devMethod.SetParameter(1, "custom");

            _devMethod.ResetToDefaults();
            var parameters = _devMethod.GetCurrentParameters();

            Assert.That(parameters[0], Is.EqualTo(100));
            Assert.That(parameters[1], Is.EqualTo("default"));
        }

        [Test]
        public void Invoke_ShouldUseStoredParameters_WhenNoParametersSupplied()
        {
            _devMethod.SetParameter(0, 250);
            _devMethod.SetParameter(1, "test");

            _devMethod.Invoke();

            Assert.That(_testInstance.LastAmount, Is.EqualTo(250));
            Assert.That(_testInstance.LastMessage, Is.EqualTo("test"));
        }

        [Test]
        public void Invoke_ShouldUseSuppliedParameters_WhenParametersSupplied()
        {
            _devMethod.SetParameter(0, 999);
            _devMethod.SetParameter(1, "stored");

            _devMethod.Invoke(new object[] { 123, "supplied" });

            Assert.That(_testInstance.LastAmount, Is.EqualTo(123));
            Assert.That(_testInstance.LastMessage, Is.EqualTo("supplied"));
        }

        [Test]
        public void SetParameter_ShouldHandleInvalidIndex()
        {
            Assert.DoesNotThrow(() => _devMethod.SetParameter(-1, 123));
            Assert.DoesNotThrow(() => _devMethod.SetParameter(999, 123));
        }

        private class TestClass
        {
            public int LastAmount { get; private set; }
            public string LastMessage { get; private set; }

            [DevMenu( KeyCode.G, "Test Group")]
            public void TestMethod(int amount = 100, string message = "default")
            {
                LastAmount = amount;
                LastMessage = message;
            }
        }
    }
} 