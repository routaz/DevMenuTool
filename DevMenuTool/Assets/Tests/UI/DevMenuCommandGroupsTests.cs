using System.Linq;
using NUnit.Framework;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Tests.TestHelpers;
using DevMenuTool.UI;

namespace DevMenuTool.Tests.UI
{
    [TestFixture]
    public class DevMenuCommandGroupsTests
    {
        [Test]
        public void GroupsAreSortedByName_CaseInsensitive()
        {
            var groups = DevMenuCommandGroups.Build(new IDevMethod[]
            {
                new MockDevMethod("A", "docking"),
                new MockDevMethod("B", "Carriers"),
                new MockDevMethod("C", "economy"),
            });

            CollectionAssert.AreEqual(new[] { "Carriers", "docking", "economy" }, groups.Select(g => g.Name).ToArray());
        }

        [Test]
        public void CommandsAreSortedByLabelWithinTheirGroup()
        {
            var groups = DevMenuCommandGroups.Build(new IDevMethod[]
            {
                new MockDevMethod("Release Pads", "Docking"),
                new MockDevMethod("fill Pads", "Docking"),
            });

            Assert.AreEqual(1, groups.Count);
            CollectionAssert.AreEqual(new[] { "fill Pads", "Release Pads" },
                groups[0].Methods.Select(m => m.ButtonLabel).ToArray());
        }

        [Test]
        public void EmptyGroupName_GoesToGeneral()
        {
            var groups = DevMenuCommandGroups.Build(new IDevMethod[] { new MockDevMethod("X", "") });
            Assert.AreEqual(DevMenuCommandGroups.DefaultGroupName, groups[0].Name);
        }

        [Test]
        public void NoCommands_GivesNoGroups()
        {
            Assert.AreEqual(0, DevMenuCommandGroups.Build(new IDevMethod[0]).Count);
        }
    }
}
