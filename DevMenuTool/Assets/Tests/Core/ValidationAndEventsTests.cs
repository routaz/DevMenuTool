using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Core;
using DevMenuTool.Core.Attributes;
using DevMenuTool.Core.Validators;
using UnityEngine;

#if UNITY_EDITOR
using DevMenuTool.Runtime.Persistence;
#endif

namespace DevMenuTool.Tests
{
    [TestFixture]
    public class ValidationAndEventsTests
    {
        private IDevMethod _devMethod;
        private TestClass _testInstance;
        private bool _eventTriggered;
        private ParameterChangeEventArgs _lastEventArgs;
        private List<ParameterChangeEventArgs> _allEvents;

        [SetUp]
        public void SetUp()
        {
            _testInstance = new TestClass();
            var methodInfo = typeof(TestClass).GetMethod("TestMethod");
            var attribute = new DevMenuAttribute(KeyCode.G, "Test Group");
            _devMethod = new DevMethodFactory().CreateDevMethod(methodInfo, _testInstance, attribute);
            
            _eventTriggered = false;
            _lastEventArgs = null;
            _allEvents = new List<ParameterChangeEventArgs>();
            
            _devMethod.ParameterChanged += OnParameterChanged;
            
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

        #region Validation Tests

        [Test]
        public void AddValidator_ShouldAddValidatorToParameter()
        {
            var validator = new RangeValidator(0, 100);

            _devMethod.AddValidator(0, validator);

            var result = _devMethod.ValidateParameter(0, 50);
            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void ValidateParameter_WithValidValue_ShouldReturnSuccess()
        {
            var validator = new RangeValidator(0, 100);
            _devMethod.AddValidator(0, validator);

            var result = _devMethod.ValidateParameter(0, 50);

            Assert.That(result.IsValid, Is.True);
            Assert.That(result.ErrorMessage, Is.Null);
        }

        [Test]
        public void ValidateParameter_WithInvalidValue_ShouldReturnFailure()
        {
            var validator = new RangeValidator(0, 100);
            _devMethod.AddValidator(0, validator);

            var result = _devMethod.ValidateParameter(0, 150);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorMessage, Is.Not.Null);
            Assert.That(result.ErrorMessage, Contains.Substring("must be between"));
        }

        [Test]
        public void SetParameter_WithValidValue_ShouldSucceed()
        {
            var validator = new RangeValidator(0, 100);
            _devMethod.AddValidator(0, validator);

            _devMethod.SetParameter(0, 50);

            var parameters = _devMethod.GetCurrentParameters();
            Assert.That(parameters[0], Is.EqualTo(50));
        }

        [Test]
        public void SetParameter_WithInvalidValue_ShouldFail()
        {
            var validator = new RangeValidator(0, 100);
            _devMethod.AddValidator(0, validator);

            _devMethod.SetParameter(0, 150);

            var parameters = _devMethod.GetCurrentParameters();
            Assert.That(parameters[0], Is.EqualTo(100));
        }

        [Test]
        public void RemoveValidator_ShouldRemoveValidator()
        {
            var validator = new RangeValidator(0, 100);
            _devMethod.AddValidator(0, validator);

            _devMethod.RemoveValidator(0, validator);

            var result = _devMethod.ValidateParameter(0, 150);
            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void ClearValidators_ShouldRemoveAllValidators()
        {
            var validator1 = new RangeValidator(0, 100);
            var validator2 = new StringValidator(minLength: 2);
            _devMethod.AddValidator(0, validator1);
            _devMethod.AddValidator(0, validator2);

            _devMethod.ClearValidators(0);

            var result = _devMethod.ValidateParameter(0, 150);
            Assert.That(result.IsValid, Is.True);
        }

        #endregion

        #region Event Tests

        [Test]
        public void SetParameter_ShouldTriggerParameterChangedEvent()
        {
            _devMethod.SetParameter(0, 50);

            Assert.That(_eventTriggered, Is.True);
            Assert.That(_lastEventArgs, Is.Not.Null);
            Assert.That(_lastEventArgs.MethodName, Is.EqualTo("TestMethod"));
            Assert.That(_lastEventArgs.ParameterName, Is.EqualTo("amount"));
            Assert.That(_lastEventArgs.OldValue, Is.EqualTo(100));
            Assert.That(_lastEventArgs.NewValue, Is.EqualTo(50));
        }

        [Test]
        public void ResetToDefaults_ShouldTriggerMultipleEvents()
        {
            _devMethod.SetParameter(0, 50);
            _devMethod.SetParameter(1, "custom");
            _eventTriggered = false;
            _lastEventArgs = null;
            _allEvents.Clear();

            _devMethod.ResetToDefaults();

            Assert.That(_eventTriggered, Is.True);
            Assert.That(_allEvents.Count, Is.EqualTo(2));
            
            var amountEvent = _allEvents.FirstOrDefault(e => e.ParameterName == "amount");
            Assert.That(amountEvent, Is.Not.Null);
            Assert.That(amountEvent.NewValue, Is.EqualTo(100));
        }

        [Test]
        public void AddParameterChangeHandler_ShouldRegisterHandler()
        {
            var handler = new TestParameterChangeHandler();
            _devMethod.AddParameterChangeHandler(handler);

            _devMethod.SetParameter(0, 75);

            Assert.That(handler.WasCalled, Is.True);
            Assert.That(handler.LastEventArgs, Is.Not.Null);
        }

        [Test]
        public void RemoveParameterChangeHandler_ShouldUnregisterHandler()
        {
            var handler = new TestParameterChangeHandler();
            _devMethod.AddParameterChangeHandler(handler);
            _devMethod.RemoveParameterChangeHandler(handler);

            _devMethod.SetParameter(0, 75);

            Assert.That(handler.WasCalled, Is.False);
        }

        #endregion

        #region Attribute Tests

        [Test]
        public void DevMethodFactory_ShouldApplyValidationAttributes()
        {
            var methodInfo = typeof(TestClassWithAttributes).GetMethod("TestMethodWithAttributes");
            var attribute = new DevMenuAttribute(KeyCode.G, "Test Group");
            var factory = new DevMethodFactory();

            var devMethod = factory.CreateDevMethod(methodInfo, new TestClassWithAttributes(), attribute);

            var result = devMethod.ValidateParameter(0, 150);
            Assert.That(result.IsValid, Is.False);
        }

        #endregion

        #region Helper Methods

        private void OnParameterChanged(object sender, ParameterChangeEventArgs e)
        {
            _eventTriggered = true;
            _lastEventArgs = e;
            _allEvents.Add(e);
        }

        #endregion

        #region Test Classes

        private class TestClass
        {
            public int LastAmount { get; private set; }
            public string LastMessage { get; private set; }

            [DevMenu(KeyCode.G, "Test Group")]
            public void TestMethod(int amount = 100, string message = "default")
            {
                LastAmount = amount;
                LastMessage = message;
            }
        }

        private class TestClassWithAttributes
        {
            [DevMenu(KeyCode.G, "Test Group")]
            public void TestMethodWithAttributes(
                [ParameterRange(0, 100)] int amount = 100,
                [StringValidation(minLength: 2)] string message = "default")
            {
            }
        }

        private class TestParameterChangeHandler : IParameterChangeHandler
        {
            public bool WasCalled { get; private set; }
            public ParameterChangeEventArgs LastEventArgs { get; private set; }

            public void OnParameterChanged(object sender, ParameterChangeEventArgs e)
            {
                WasCalled = true;
                LastEventArgs = e;
            }
        }

        #endregion
    }
} 