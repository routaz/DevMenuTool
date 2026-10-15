using NUnit.Framework;
using UnityEngine;
using DevMenuTool.Runtime;

namespace DevMenuTool.Tests.Runtime
{
    [TestFixture]
    public class DevMenuKeyMapTests
    {
        [TestCase(KeyCode.F1, "F1")]
        [TestCase(KeyCode.F7, "F7")]
        [TestCase(KeyCode.F12, "F12")]
        [TestCase(KeyCode.BackQuote, "Backquote")]
        public void SupportedKeys_MapToInputSystemKeyNames(KeyCode keyCode, string expected)
        {
            Assert.IsTrue(DevMenuKeyMap.TryGetInputSystemKeyName(keyCode, out var name));
            Assert.AreEqual(expected, name);
        }

        [TestCase(KeyCode.A)]
        [TestCase(KeyCode.F13)]
        [TestCase(KeyCode.None)]
        public void UnsupportedKeys_AreRejected(KeyCode keyCode)
        {
            Assert.IsFalse(DevMenuKeyMap.TryGetInputSystemKeyName(keyCode, out var name));
            Assert.IsNull(name);
        }
    }
}
