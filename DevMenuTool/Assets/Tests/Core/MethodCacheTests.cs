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
            var mockMethod = CreateMockDevMethod("TestMethod");

            _methodCache.AddMethod(mockMethod);

            Assert.That(_methodCache.CachedMethods.Count, Is.EqualTo(1));
            Assert.That(_methodCache.CachedMethods[0], Is.EqualTo(mockMethod));
        }

        [Test]
        public void AddMethod_WithNullMethod_ShouldNotAddToCache()
        {
            _methodCache.AddMethod(null);

            Assert.That(_methodCache.CachedMethods.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddMethod_WithDuplicateMethod_ShouldNotAddDuplicate()
        {
            var mockMethod = CreateMockDevMethod("TestMethod");
            _methodCache.AddMethod(mockMethod);

            _methodCache.AddMethod(mockMethod);

            Assert.That(_methodCache.CachedMethods.Count, Is.EqualTo(1));
        }

        [Test]
        public void Clear_ShouldRemoveAllMethods()
        {
            _methodCache.AddMethod(CreateMockDevMethod("Method1"));
            _methodCache.AddMethod(CreateMockDevMethod("Method2"));

            _methodCache.Clear();

            Assert.That(_methodCache.CachedMethods.Count, Is.EqualTo(0));
        }

        [Test]
        public void Refresh_ShouldClearCache()
        {
            _methodCache.AddMethod(CreateMockDevMethod("Method1"));

            _methodCache.Refresh();

            Assert.That(_methodCache.CachedMethods.Count, Is.EqualTo(0));
        }

        private IDevMethod CreateMockDevMethod(string methodName)
        {
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
            }

            public object[] GetCurrentParameters()
            {
                return new object[0];
            }

            public void SetParameter(int index, object value)
            {
            }

            public void ResetToDefaults()
            {
            }

            public void SaveToEditorPrefs()
            {
                Debug.Log($"Mock: Saving parameters for method {MethodName}");
            }

            public bool LoadFromEditorPrefs()
            {
                Debug.Log($"Mock: Loading parameters for method {MethodName}");
                return true;
            }

            public void AddValidator(int parameterIndex, IParameterValidator validator)
            {
            }

            public void RemoveValidator(int parameterIndex, IParameterValidator validator)
            {
            }

            public void ClearValidators(int parameterIndex)
            {
            }

            public ParameterValidationResult ValidateParameter(int parameterIndex, object value)
            {
                return ParameterValidationResult.Success();
            }

            public event EventHandler<ParameterChangeEventArgs> ParameterChanged;
            
            public void AddParameterChangeHandler(IParameterChangeHandler handler)
            {
            }

            public void RemoveParameterChangeHandler(IParameterChangeHandler handler)
            {
            }
        }
    }
} 