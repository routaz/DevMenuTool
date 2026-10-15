using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using DevMenuTool.Core;
using DevMenuTool.Core.Attributes;

namespace DevMenuTool.Tests.Core
{
    [TestFixture]
    public class MethodScannerIndexTests
    {
        private const BindingFlags InstanceMethods =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        private readonly List<GameObject> _created = new();
        private DevMenuMethodIndex _index;
        private MethodScanner _scanner;

        [SetUp]
        public void Setup()
        {
            _index = new DevMenuMethodIndex();
            _index.Set(typeof(ScanTargetBehaviour).GetMethods(InstanceMethods));
            _scanner = new MethodScanner(new MethodCache(), new DevMethodFactory(), _index);
        }

        [TearDown]
        public void Cleanup()
        {
            foreach (var go in _created)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }
            _created.Clear();
        }

        [Test]
        public void ScanScene_FindsActiveInactiveAndHiddenInstancesOfIndexedType()
        {
            var active = Create<ScanTargetBehaviour>("Active");
            var inactive = Create<ScanTargetBehaviour>("Inactive");
            inactive.gameObject.SetActive(false);
            var hidden = Create<ScanTargetBehaviour>("Hidden");
            hidden.gameObject.hideFlags = HideFlags.HideAndDontSave;

            _scanner.ScanScene();

            var targets = TargetsOf(nameof(ScanTargetBehaviour.ScanCommand));
            Assert.That(targets, Does.Contain(active));
            Assert.That(targets, Does.Contain(inactive));
            Assert.That(targets, Does.Contain(hidden));
        }

        [Test]
        public void ScanScene_IgnoresInstancesOfTypesNotInIndex()
        {
            var notIndexed = Create<NotIndexedBehaviour>("NotIndexed");

            _scanner.ScanScene();

            Assert.That(_scanner.CachedMethods.Any(m => ReferenceEquals(m.TargetInstance, notIndexed)), Is.False);
            Assert.That(_scanner.CachedMethods.Any(m => m.MethodInfo.Name == nameof(NotIndexedBehaviour.NotIndexedCommand)), Is.False);
        }

        [Test]
        public void ScanScene_SubclassInstanceGetsBaseTypeMethod()
        {
            var subclass = Create<ScanTargetSubclass>("Subclass");

            _scanner.ScanScene();

            var entries = _scanner.CachedMethods.Where(m => ReferenceEquals(m.TargetInstance, subclass)).ToList();
            Assert.That(entries.Count, Is.EqualTo(1));
            Assert.That(entries[0].MethodInfo.Name, Is.EqualTo(nameof(ScanTargetBehaviour.ScanCommand)));
        }

        [Test]
        public void RebuildIndex_RaisesRebuilt()
        {
            var raised = 0;
            _index.Rebuilt += () => raised++;

            _scanner.RebuildIndex();

            Assert.That(raised, Is.EqualTo(1));
            Assert.That(_index.Methods.Any(m => m.DeclaringType == typeof(NotIndexedBehaviour)), Is.True,
                "RebuildIndex should rebuild from the loaded assemblies");
        }

        [Test]
        public void ScanScene_OnUnbuiltIndex_DoesNotRaiseRebuiltOrDuplicateMethods()
        {
            var index = new DevMenuMethodIndex();
            var scanner = new MethodScanner(new MethodCache(), new DevMethodFactory(), index);
            var raised = 0;
            index.Rebuilt += () =>
            {
                raised++;
                scanner.ScanScene();
            };
            var target = Create<ScanTargetBehaviour>("LazyBuild");

            scanner.ScanScene();

            Assert.That(index.IsBuilt, Is.True);
            Assert.That(raised, Is.EqualTo(0));
            Assert.That(scanner.CachedMethods.Count(m => ReferenceEquals(m.TargetInstance, target)), Is.EqualTo(1));
        }

        private T Create<T>(string name) where T : MonoBehaviour
        {
            var go = new GameObject(name);
            _created.Add(go);
            return go.AddComponent<T>();
        }

        private List<object> TargetsOf(string methodName)
        {
            return _scanner.CachedMethods
                .Where(m => m.MethodInfo.Name == methodName)
                .Select(m => m.TargetInstance)
                .ToList();
        }

        public class ScanTargetBehaviour : MonoBehaviour
        {
            [DevMenu("ScanGroup")]
            public void ScanCommand()
            {
            }
        }

        public class ScanTargetSubclass : ScanTargetBehaviour
        {
        }

        public class NotIndexedBehaviour : MonoBehaviour
        {
            [DevMenu("ScanGroup")]
            public void NotIndexedCommand()
            {
            }
        }
    }
}
