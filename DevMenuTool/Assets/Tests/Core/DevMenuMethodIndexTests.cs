using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using DevMenuTool.Core;
using DevMenuTool.Core.Attributes;

namespace DevMenuTool.Tests.Core
{
    [TestFixture]
    public class DevMenuMethodIndexTests
    {
        private const BindingFlags AllMethods =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        [Test]
        public void Set_IndexesOnlyInstanceMethodsWithDevMenuAttribute()
        {
            var index = new DevMenuMethodIndex();

            index.Set(typeof(IndexedBehaviour).GetMethods(AllMethods));

            Assert.That(index.IsBuilt, Is.True);
            Assert.That(index.Methods.Count, Is.EqualTo(1));
            var entry = index.Methods[0];
            Assert.That(entry.Method.Name, Is.EqualTo(nameof(IndexedBehaviour.IndexedCommand)));
            Assert.That(entry.DeclaringType, Is.EqualTo(typeof(IndexedBehaviour)));
            Assert.That(entry.Attribute.GroupName, Is.EqualTo("IndexGroup"));
            Assert.That(entry.Attribute.ShortcutKey, Is.EqualTo(KeyCode.F7));
        }

        [Test]
        public void IsBuilt_IsFalseBeforeBuilding()
        {
            Assert.That(new DevMenuMethodIndex().IsBuilt, Is.False);
        }

        [Test]
        public void BuildFromAssemblies_FindsAttributedMethodInTestAssembly()
        {
            var index = new DevMenuMethodIndex();

            index.BuildFromAssemblies(new[] { typeof(IndexedBehaviour).Assembly });

            Assert.That(index.IsBuilt, Is.True);
            Assert.That(index.Methods.Any(m =>
                m.DeclaringType == typeof(IndexedBehaviour) && m.Method.Name == nameof(IndexedBehaviour.IndexedCommand)), Is.True);
            Assert.That(index.Methods.Any(m => m.Method.Name == nameof(IndexedBehaviour.StaticCommand)), Is.False);
        }

        [Test]
        public void BuildFromAssemblies_SkipsAssemblyThatDoesNotReferenceDevMenuTool()
        {
            var index = new DevMenuMethodIndex();

            index.BuildFromAssemblies(new[] { typeof(object).Assembly });

            Assert.That(DevMenuMethodIndex.IsCandidateAssembly(typeof(object).Assembly), Is.False);
            Assert.That(DevMenuMethodIndex.IsCandidateAssembly(typeof(IndexedBehaviour).Assembly), Is.True);
            Assert.That(DevMenuMethodIndex.IsCandidateAssembly(typeof(DevMenuAttribute).Assembly), Is.True);
            Assert.That(index.IsBuilt, Is.True);
            Assert.That(index.Methods, Is.Empty);
        }

        [Test]
        public void EnsureBuilt_BuildsSynchronouslyWhenNoBackgroundBuildWasStarted()
        {
            var index = new DevMenuMethodIndex();

            index.EnsureBuilt();

            Assert.That(index.IsBuilt, Is.True);
            Assert.That(index.Methods.Any(m => m.DeclaringType == typeof(IndexedBehaviour)), Is.True);
        }

        [Test]
        public void BuildInBackground_ThenEnsureBuilt_WaitsForResult()
        {
            var index = new DevMenuMethodIndex();

            index.BuildInBackground();
            index.EnsureBuilt();

            Assert.That(index.IsBuilt, Is.True);
            Assert.That(index.Methods.Any(m => m.DeclaringType == typeof(IndexedBehaviour)), Is.True);
        }

        public class IndexedBehaviour : MonoBehaviour
        {
            public int CallCount { get; private set; }

            [DevMenu(KeyCode.F7, "IndexGroup")]
            public void IndexedCommand()
            {
                CallCount++;
            }

            public void PlainMethod()
            {
            }

            [DevMenu("IndexGroup")]
            public static void StaticCommand()
            {
            }
        }
    }
}
