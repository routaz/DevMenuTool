using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using DevMenuTool.Core;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Core.Attributes;
using UnityEngine;

namespace DevMenuTool.Tests.Core
{
    [TestFixture]
    public class MethodCacheTests
    {
        private IMethodCache _methodCache;

        [SetUp]
        public void Setup()
        {
            _methodCache = new MethodCache();
        }

        [Test]
        public void AddMethod_WithValidMethod_ShouldAddToCache()
        {
            // Arrange
            var mockMethod = CreateMockDevMethod("TestMethod");

            // Act
            _methodCache.AddMethod(mockMethod);

            // Assert
            Assert.That(_methodCache.CachedMethods.Count, Is.EqualTo(1));
            Assert.That(_methodCache.CachedMethods[0], Is.EqualTo(mockMethod));
        }

        [Test]
        public void AddMethod_WithNullMethod_ShouldNotAddToCache()
        {
            // Act
            _methodCache.AddMethod(null);

            // Assert
            Assert.That(_methodCache.CachedMethods.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddMethod_WithDuplicateMethod_ShouldNotAddDuplicate()
        {
            // Arrange
            var mockMethod = CreateMockDevMethod("TestMethod");
            _methodCache.AddMethod(mockMethod);

            // Act
            _methodCache.AddMethod(mockMethod);

            // Assert
            Assert.That(_methodCache.CachedMethods.Count, Is.EqualTo(1));
        }

        [Test]
        public void Clear_ShouldRemoveAllMethods()
        {
            // Arrange
            _methodCache.AddMethod(CreateMockDevMethod("Method1"));
            _methodCache.AddMethod(CreateMockDevMethod("Method2"));

            // Act
            _methodCache.Clear();

            // Assert
            Assert.That(_methodCache.CachedMethods.Count, Is.EqualTo(0));
        }

        [Test]
        public void Refresh_ShouldClearCache()
        {
            // Arrange
            _methodCache.AddMethod(CreateMockDevMethod("Method1"));

            // Act
            _methodCache.Refresh();

            // Assert
            Assert.That(_methodCache.CachedMethods.Count, Is.EqualTo(0));
        }

        private IDevMethod CreateMockDevMethod(string methodName)
        {
            // Create a simple mock implementation for testing
            return new MockDevMethod(methodName);
        }

        private class MockDevMethod : IDevMethod
        {
            public string GroupName => "TestGroup";
            public string ButtonLabel => MethodName;
            public KeyCode ShortcutKey => KeyCode.None;
            public MethodInfo MethodInfo => null;
            public object TargetInstance => null;
            public ParameterInfo[] Parameters => new ParameterInfo[0];
            public string MethodName { get; }

            public MockDevMethod(string methodName)
            {
                MethodName = methodName;
            }

            public void Invoke(object[] parameters = null)
            {
                // Mock implementation
            }

            public object[] GetCurrentParameters()
            {
                return new object[0]; // No parameters for this mock
            }

            public void SetParameter(int index, object value)
            {
                // Mock implementation - not used in cache tests
            }

            public void ResetToDefaults()
            {
                // Mock implementation - not used in cache tests
            }

            public void SaveToEditorPrefs()
            {
                // Mock implementation - not used in cache tests
                Debug.Log($"Mock: Saving parameters for method {MethodName}");
            }

            public bool LoadFromEditorPrefs()
            {
                // Mock implementation - not used in cache tests
                Debug.Log($"Mock: Loading parameters for method {MethodName}");
                return true;
            }

            public void AddValidator(int parameterIndex, IParameterValidator validator)
            {
                // Mock implementation - not used in cache tests
            }

            public void RemoveValidator(int parameterIndex, IParameterValidator validator)
            {
                // Mock implementation - not used in cache tests
            }

            public void ClearValidators(int parameterIndex)
            {
                // Mock implementation - not used in cache tests
            }

            public ParameterValidationResult ValidateParameter(int parameterIndex, object value)
            {
                // Mock implementation - always return success
                return ParameterValidationResult.Success();
            }

            public event EventHandler<ParameterChangeEventArgs> ParameterChanged;
            
            public void AddParameterChangeHandler(IParameterChangeHandler handler)
            {
                // Mock implementation - not used in cache tests
            }

            public void RemoveParameterChangeHandler(IParameterChangeHandler handler)
            {
                // Mock implementation - not used in cache tests
            }
        }
    }
} 