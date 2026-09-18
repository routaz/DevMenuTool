using System.Linq;
using NUnit.Framework;
using UnityEngine;
using DevMenuTool.Core;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Runtime;
using DevMenuTool.Core.Attributes;

#if UNITY_EDITOR
using DevMenuTool.Runtime.Persistence;
#endif

namespace DevMenuTool.Tests.Integration
{
    [TestFixture]
    public class DevMenuIntegrationTests
    {
        private IMethodScanner _scanner;
        private IMethodInvoker _invoker;
        private TestMonoBehaviour _testObject;

        [SetUp]
        public void Setup()
        {
            _scanner = DevMenuContainer.MethodScanner;
            _invoker = DevMenuContainer.MethodInvoker;
            
            // Create a test GameObject with our test component
            var go = new GameObject("TestObject");
            _testObject = go.AddComponent<TestMonoBehaviour>();
            
            // Enable test mode to isolate test data from development data
            #if UNITY_EDITOR
            DevMenuParameterPrefs.SetTestMode(true);
            DevMenuParameterPrefs.ClearAll();
            #endif
        }

        [TearDown]
        public void Cleanup()
        {
            DevMenuContainer.Reset();
            if (_testObject != null)
            {
                Object.DestroyImmediate(_testObject.gameObject);
            }
            
            // Disable test mode and clean up test data
            #if UNITY_EDITOR
            DevMenuParameterPrefs.ClearAll();
            DevMenuParameterPrefs.SetTestMode(false);
            #endif
        }

        [Test]
        public void CompleteWorkflow_ShouldFindAndInvokeMethods()
        {
            // Arrange
            _testObject.ResetState();

            // Act
            _scanner.ScanScene();

            // Assert
            Assert.That(_scanner.CachedMethods.Count, Is.GreaterThan(0));
            
            var testMethod = _scanner.CachedMethods.FirstOrDefault(m => m.ButtonLabel == "TestMethod");
            Assert.That(testMethod, Is.Not.Null);
            Assert.That(testMethod.GroupName, Is.EqualTo("TestGroup"));
            Assert.That(testMethod.ShortcutKey, Is.EqualTo(KeyCode.T));

            // Invoke the method
            _invoker.Invoke(testMethod);
            Assert.That(_testObject.MethodCalled, Is.True);
        }

        [Test]
        public void CompleteWorkflow_WithParameters_ShouldInvokeWithCorrectParameters()
        {
            // Arrange
            _testObject.ResetState();

            // Act
            _scanner.ScanScene();
            var methodWithParams = _scanner.CachedMethods.FirstOrDefault(m => m.ButtonLabel == "TestMethodWithParams");
            
            Assert.That(methodWithParams, Is.Not.Null);
            Assert.That(methodWithParams.Parameters.Length, Is.EqualTo(2));

            // Invoke with parameters
            _invoker.Invoke(methodWithParams, new object[] { 42, "test" });

            // Assert
            Assert.That(_testObject.MethodCalled, Is.True);
            Assert.That(_testObject.LastIntParam, Is.EqualTo(42));
            Assert.That(_testObject.LastStringParam, Is.EqualTo("test"));
        }

        [Test]
        public void CompleteWorkflow_WithDefaultParameters_ShouldUseDefaults()
        {
            // Arrange
            _testObject.ResetState();

            // Act
            _scanner.ScanScene();
            var methodWithDefaults = _scanner.CachedMethods.FirstOrDefault(m => m.ButtonLabel == "TestMethodWithDefaults");
            
            Assert.That(methodWithDefaults, Is.Not.Null);
            Assert.That(methodWithDefaults.Parameters.Length, Is.EqualTo(2));

            // Force reset to defaults to ensure clean state
            methodWithDefaults.ResetToDefaults();

            // Invoke without parameters (should use defaults)
            _invoker.Invoke(methodWithDefaults);

            // Assert
            Assert.That(_testObject.MethodCalled, Is.True);
            Assert.That(_testObject.LastIntParam, Is.EqualTo(10)); // Default value
            Assert.That(_testObject.LastStringParam, Is.EqualTo("default")); // Default value
        }

        [Test]
        public void CompleteWorkflow_WithMixedParameters_ShouldUseSuppliedAndDefaults()
        {
            // Arrange
            _testObject.ResetState();

            // Act
            _scanner.ScanScene();
            var methodWithDefaults = _scanner.CachedMethods.FirstOrDefault(m => m.ButtonLabel == "TestMethodWithDefaults");
            
            // Invoke with only first parameter
            _invoker.Invoke(methodWithDefaults, new object[] { 100 });

            // Assert
            Assert.That(_testObject.MethodCalled, Is.True);
            Assert.That(_testObject.LastIntParam, Is.EqualTo(100)); // Supplied value
            Assert.That(_testObject.LastStringParam, Is.EqualTo("default")); // Default value
        }

        [Test]
        public void CompleteWorkflow_WithMultipleObjects_ShouldFindAllMethods()
        {
            // Arrange
            var go2 = new GameObject("TestObject2");
            var testObject2 = go2.AddComponent<TestMonoBehaviour>();

            try
            {
                // Act
                _scanner.ScanScene();

                // Assert
                Assert.That(_scanner.CachedMethods.Count, Is.GreaterThan(0));
                
                // Should find methods from both objects
                var methodsFromObject1 = _scanner.CachedMethods.Where(m => m.TargetInstance == _testObject);
                var methodsFromObject2 = _scanner.CachedMethods.Where(m => m.TargetInstance == testObject2);
                
                Assert.That(methodsFromObject1.Count(), Is.GreaterThan(0));
                Assert.That(methodsFromObject2.Count(), Is.GreaterThan(0));
            }
            finally
            {
                Object.DestroyImmediate(go2);
            }
        }

        [Test]
        public void CompleteWorkflow_WithEnumParameter_ShouldHandleEnumCorrectly()
        {
            // Arrange
            _testObject.ResetState();

            // Act
            _scanner.ScanScene();
            var methodWithEnum = _scanner.CachedMethods.FirstOrDefault(m => m.ButtonLabel == "TestMethodWithEnum");
            
            Assert.That(methodWithEnum, Is.Not.Null);
            Assert.That(methodWithEnum.Parameters.Length, Is.EqualTo(1));
            Assert.That(methodWithEnum.Parameters[0].ParameterType.IsEnum, Is.True);

            // Invoke with enum parameter
            _invoker.Invoke(methodWithEnum, new object[] { TestEnum.Value2 });

            // Assert
            Assert.That(_testObject.MethodCalled, Is.True);
            Assert.That(_testObject.LastEnumParam, Is.EqualTo(TestEnum.Value2));
        }

        [Test]
        public void ScanScene_ShouldFindMethodsOnHideAndDontSaveObjects()
        {
            // Arrange
            _testObject.gameObject.hideFlags = HideFlags.HideAndDontSave;

            // Act
            _scanner.ScanScene();

            // Assert
            var testMethod = _scanner.CachedMethods.FirstOrDefault(m => m.ButtonLabel == "TestMethod");
            Assert.That(testMethod, Is.Not.Null);
            Assert.That(testMethod.TargetInstance, Is.SameAs(_testObject));
        }

        [Test]
        public void DevMenuAttribute_WithGroupOnly_ShouldHaveNoHotkey()
        {
            // Act
            _scanner.ScanScene();

            // Assert
            var menuOnlyMethod = _scanner.CachedMethods.FirstOrDefault(m => m.ButtonLabel == "MenuOnlyMethod");
            Assert.That(menuOnlyMethod, Is.Not.Null);
            Assert.That(menuOnlyMethod.GroupName, Is.EqualTo("TestGroup"));
            Assert.That(menuOnlyMethod.ShortcutKey, Is.EqualTo(KeyCode.None));
        }

        public class TestMonoBehaviour : MonoBehaviour
        {
            public bool MethodCalled { get; private set; }
            public int LastIntParam { get; private set; }
            public string LastStringParam { get; private set; }
            public TestEnum LastEnumParam { get; private set; }

            public void ResetState()
            {
                MethodCalled = false;
                LastIntParam = 0;
                LastStringParam = string.Empty;
                LastEnumParam = TestEnum.Value1;
            }

            [DevMenu(KeyCode.T, "TestGroup")]
            private void TestMethod()
            {
                MethodCalled = true;
            }

            [DevMenu(KeyCode.Y, "TestGroup")]
            private void TestMethodWithParams(int param1, string param2)
            {
                MethodCalled = true;
                LastIntParam = param1;
                LastStringParam = param2;
            }

            [DevMenu(KeyCode.U, "TestGroup")]
            private void TestMethodWithDefaults(int param1 = 10, string param2 = "default")
            {
                MethodCalled = true;
                LastIntParam = param1;
                LastStringParam = param2;
            }

            [DevMenu(KeyCode.I, "TestGroup")]
            private void TestMethodWithEnum(TestEnum enumParam)
            {
                MethodCalled = true;
                LastEnumParam = enumParam;
            }

            [DevMenu("TestGroup")]
            private void MenuOnlyMethod()
            {
                MethodCalled = true;
            }
        }

        public enum TestEnum
        {
            Value1,
            Value2,
            Value3
        }
    }
} 