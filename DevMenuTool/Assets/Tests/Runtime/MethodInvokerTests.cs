using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using DevMenuTool.Runtime;
using DevMenuTool.Core.Interfaces;
using UnityEngine;
using UnityEngine.TestTools;

namespace DevMenuTool.Tests.Runtime
{
    [TestFixture]
    public class MethodInvokerTests
    {
        private MethodInvoker _invoker;
        private TestClass _testInstance;

        [SetUp]
        public void Setup()
        {
            _invoker = new MethodInvoker();
            _testInstance = new TestClass();
        }

        [Test]
        public void Invoke_WithMethodWithoutParameters_ShouldInvokeMethod()
        {
            // Arrange
            var method = CreateMockDevMethod("TestMethod");

            // Act
            _invoker.Invoke(method);

            // Assert
            Assert.That(_testInstance.MethodCalled, Is.True);
        }

        [Test]
        public void Invoke_WithMethodWithParameters_ShouldUseDefaultValues()
        {
            // Arrange
            var method = CreateMockDevMethod("TestMethodWithParams");

            // Act
            _invoker.Invoke(method);

            // Assert
            Assert.That(_testInstance.MethodCalled, Is.True);
            // MethodInvoker will use default values (0 for int, null for string)
            Assert.That(_testInstance.LastIntParam, Is.EqualTo(0));
            Assert.That(_testInstance.LastStringParam, Is.Null);
        }

        [Test]
        public void Invoke_WithMethodWithDefaultParameters_ShouldUseDefaults()
        {
            // Arrange
            var method = CreateMockDevMethod("TestMethodWithDefaults");

            // Act
            _invoker.Invoke(method);

            // Assert
            Assert.That(_testInstance.MethodCalled, Is.True);
            Assert.That(_testInstance.LastIntParam, Is.EqualTo(10)); // Default value
            Assert.That(_testInstance.LastStringParam, Is.EqualTo("default")); // Default value
        }

        [Test]
        public void Invoke_WithMethodWithSuppliedArgs_ShouldUseSuppliedArgs()
        {
            // Arrange
            var method = CreateMockDevMethod("TestMethodWithParams");
            var suppliedArgs = new object[] { 100, "supplied" };

            // Act
            _invoker.Invoke(method, suppliedArgs);

            // Assert
            Assert.That(_testInstance.MethodCalled, Is.True);
            Assert.That(_testInstance.LastIntParam, Is.EqualTo(100));
            Assert.That(_testInstance.LastStringParam, Is.EqualTo("supplied"));
        }

        [Test]
        public void Invoke_WithMethodWithMixedArgs_ShouldUseSuppliedArgsAndDefaults()
        {
            // Arrange
            var method = CreateMockDevMethod("TestMethodWithDefaults");
            var suppliedArgs = new object[] { 200 }; // Only supply first parameter

            // Act
            _invoker.Invoke(method, suppliedArgs);

            // Assert
            Assert.That(_testInstance.MethodCalled, Is.True);
            Assert.That(_testInstance.LastIntParam, Is.EqualTo(200)); // Supplied value
            Assert.That(_testInstance.LastStringParam, Is.EqualTo("default")); // Default value
        }

        [Test]
        public void Invoke_WithMethodThatThrowsException_ShouldLogError()
        {
            // Arrange
            var method = CreateMockDevMethod("TestMethodThatThrows");
            
            // Expect the error log message BEFORE the action that generates it
            LogAssert.Expect(LogType.Error, "Error invoking method TestMethodThatThrows: Test exception");

            // Act & Assert
            Assert.DoesNotThrow(() => _invoker.Invoke(method));
        }

        private IDevMethod CreateMockDevMethod(string methodName)
        {
            return new MockDevMethod(_testInstance, methodName);
        }

        public class TestClass
        {
            public bool MethodCalled { get; private set; }
            public int LastIntParam { get; private set; }
            public string LastStringParam { get; private set; }

            public void TestMethod()
            {
                MethodCalled = true;
            }

            public void TestMethodWithParams(int param1, string param2)
            {
                MethodCalled = true;
                LastIntParam = param1;
                LastStringParam = param2;
            }

            public void TestMethodWithDefaults(int param1 = 10, string param2 = "default")
            {
                MethodCalled = true;
                LastIntParam = param1;
                LastStringParam = param2;
            }

            public void TestMethodThatThrows()
            {
                throw new InvalidOperationException("Test exception");
            }
        }

        private class MockDevMethod : IDevMethod
        {
            private readonly TestClass _targetInstance;
            private readonly string _methodName;
            private readonly Dictionary<int, object> _parameters = new Dictionary<int, object>();

            public MockDevMethod(TestClass targetInstance, string methodName)
            {
                _targetInstance = targetInstance;
                _methodName = methodName;
            }

            public string GroupName => "TestGroup";
            public string ButtonLabel => _methodName;
            public KeyCode ShortcutKey => KeyCode.None;
            public MethodInfo MethodInfo => typeof(TestClass).GetMethod(_methodName);
            public object TargetInstance => _targetInstance;
            public ParameterInfo[] Parameters => MethodInfo?.GetParameters() ?? new ParameterInfo[0];

            public void Invoke(object[] parameters = null)
            {
                // Use supplied parameters if provided, otherwise use stored parameters
                var argsToUse = parameters ?? GetCurrentParameters();
                MethodInfo?.Invoke(TargetInstance, argsToUse);
            }

            public object[] GetCurrentParameters()
            {
                var result = new object[Parameters.Length];
                for (int i = 0; i < Parameters.Length; i++)
                {
                    if (_parameters.TryGetValue(i, out var value))
                    {
                        result[i] = value;
                    }
                    else if (Parameters[i].HasDefaultValue)
                    {
                        result[i] = Parameters[i].DefaultValue;
                    }
                    else if (Parameters[i].ParameterType.IsValueType)
                    {
                        result[i] = Activator.CreateInstance(Parameters[i].ParameterType);
                    }
                    else
                    {
                        result[i] = null;
                    }
                }
                return result;
            }

            public void SetParameter(int index, object value)
            {
                if (index >= 0 && index < Parameters.Length)
                {
                    _parameters[index] = value;
                }
            }

            public void ResetToDefaults()
            {
                _parameters.Clear();
            }

            public void SaveToEditorPrefs()
            {
                // Mock implementation
                Debug.Log($"Mock: Saving parameters for method {_methodName}");
            }

            public bool LoadFromEditorPrefs()
            {
                // Mock implementation
                Debug.Log($"Mock: Loading parameters for method {_methodName}");
                return true;
            }

            public void AddValidator(int parameterIndex, IParameterValidator validator)
            {
                // Mock implementation - not used in these tests
            }

            public void RemoveValidator(int parameterIndex, IParameterValidator validator)
            {
                // Mock implementation - not used in these tests
            }

            public void ClearValidators(int parameterIndex)
            {
                // Mock implementation - not used in these tests
            }

            public ParameterValidationResult ValidateParameter(int parameterIndex, object value)
            {
                // Mock implementation - always return success
                return ParameterValidationResult.Success();
            }

            public event EventHandler<ParameterChangeEventArgs> ParameterChanged;
            
            public void AddParameterChangeHandler(IParameterChangeHandler handler)
            {
                // Mock implementation - not used in these tests
            }

            public void RemoveParameterChangeHandler(IParameterChangeHandler handler)
            {
                // Mock implementation - not used in these tests
            }
        }
    }
} 