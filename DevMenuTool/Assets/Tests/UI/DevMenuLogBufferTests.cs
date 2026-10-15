using NUnit.Framework;
using UnityEngine;
using DevMenuTool.UI;

namespace DevMenuTool.Tests.UI
{
    [TestFixture]
    public class DevMenuLogBufferTests
    {
        [Test]
        public void KeepsOnlyTheLastFive_OldestFirst()
        {
            var buffer = new DevMenuLogBuffer();
            for (int i = 1; i <= 7; i++) buffer.Add(LogType.Log, $"line {i}");

            var entries = buffer.Snapshot();
            Assert.AreEqual(5, entries.Count);
            Assert.AreEqual("line 3", entries[0].Message);
            Assert.AreEqual("line 7", entries[4].Message);
        }

        [Test]
        public void KeepsOnlyTheFirstLine()
        {
            var buffer = new DevMenuLogBuffer();
            buffer.Add(LogType.Error, "boom\r\n  at Foo.Bar()\n  at Baz()");
            Assert.AreEqual("boom", buffer.Snapshot()[0].Message);
        }

        [Test]
        public void PreservesTheLogType()
        {
            var buffer = new DevMenuLogBuffer();
            buffer.Add(LogType.Warning, "careful");
            Assert.AreEqual(LogType.Warning, buffer.Snapshot()[0].Type);
        }

        [Test]
        public void NullMessage_BecomesEmpty()
        {
            var buffer = new DevMenuLogBuffer();
            buffer.Add(LogType.Log, null);
            Assert.AreEqual(string.Empty, buffer.Snapshot()[0].Message);
        }

        [Test]
        public void Version_ChangesOnEveryAdd()
        {
            var buffer = new DevMenuLogBuffer();
            int before = buffer.Version;
            buffer.Add(LogType.Log, "a");
            Assert.AreNotEqual(before, buffer.Version);
        }

        [Test]
        public void LogSpam_StaysBoundedAtCapacity()
        {
            var buffer = new DevMenuLogBuffer();
            for (int i = 0; i < 1000; i++) buffer.Add(LogType.Log, $"spam {i}");

            Assert.AreEqual(DevMenuLogBuffer.DefaultCapacity, buffer.Count);
            Assert.AreEqual("spam 999", buffer.Snapshot()[4].Message);
        }
    }
}
