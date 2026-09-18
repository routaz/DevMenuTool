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
            
            // Enable test mode to isolate test data from development data
            #if UNITY_EDITOR
            DevMenuParameterPrefs.SetTestMode(true);
            DevMenuParameterPrefs.ClearAll();
            #endif
            
            // Force reset to defaults to ensure clean state
            _devMethod.ResetToDefaults();
        }

        [TearDown]
        public void TearDown()
        {
            // Disable test mode and clean up test data
            #if UNITY_EDITOR
            DevMenuParameterPrefs.ClearAll();
            DevMenuParameterPrefs.SetTestMode(false);
            #endif
        }

        [Test]
        public void GetCurrentParameters_ShouldReturnDefaultValues_WhenNoParametersSet()
        {
            // Act
            var parameters = _devMethod.GetCurrentParameters();

            // Assert
            Assert.That(parameters.Length, Is.EqualTo(2));
            Assert.That(parameters[0], Is.EqualTo(100)); // Default value
            Assert.That(parameters[1], Is.EqualTo("default")); // Default value
        }

        [Test]
        public void SetParameter_ShouldUpdateParameterValue()
        {
            // Arrange
            var newValue = 500;

            // Act
            _devMethod.SetParameter(0, newValue);
            var parameters = _devMethod.GetCurrentParameters();

            // Assert
            Assert.That(parameters[0], Is.EqualTo(newValue));
        }

        [Test]
        public void ResetToDefaults_ShouldRestoreDefaultValues()
        {
            // Arrange
            _devMethod.SetParameter(0, 999);
            _devMethod.SetParameter(1, "custom");

            // Act
            _devMethod.ResetToDefaults();
            var parameters = _devMethod.GetCurrentParameters();

            // Assert
            Assert.That(parameters[0], Is.EqualTo(100)); // Back to default
            Assert.That(parameters[1], Is.EqualTo("default")); // Back to default
        }

        [Test]
        public void Invoke_ShouldUseStoredParameters_WhenNoParametersSupplied()
        {
            // Arrange
            _devMethod.SetParameter(0, 250);
            _devMethod.SetParameter(1, "test");

            // Act
            _devMethod.Invoke(); // No parameters supplied

            // Assert
            Assert.That(_testInstance.LastAmount, Is.EqualTo(250));
            Assert.That(_testInstance.LastMessage, Is.EqualTo("test"));
        }

        [Test]
        public void Invoke_ShouldUseSuppliedParameters_WhenParametersSupplied()
        {
            // Arrange
            _devMethod.SetParameter(0, 999); // Stored value
            _devMethod.SetParameter(1, "stored"); // Stored value

            // Act
            _devMethod.Invoke(new object[] { 123, "supplied" }); // Supplied values

            // Assert
            Assert.That(_testInstance.LastAmount, Is.EqualTo(123)); // Should use supplied
            Assert.That(_testInstance.LastMessage, Is.EqualTo("supplied")); // Should use supplied
        }

        [Test]
        public void SetParameter_ShouldHandleInvalidIndex()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _devMethod.SetParameter(-1, 123));
            Assert.DoesNotThrow(() => _devMethod.SetParameter(999, 123));
        }

        // Test class for our tests
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