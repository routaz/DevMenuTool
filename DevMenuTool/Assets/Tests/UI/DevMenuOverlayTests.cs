using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using DevMenuTool.Core.Attributes;
using DevMenuTool.UI;

namespace DevMenuTool.Tests.UI
{
    public class OverlayTestCommands : MonoBehaviour
    {
        public bool Flag;

        [DevMenu("OverlayTests")]
        private void SetFlag(bool flag = false) => Flag = flag;
    }

    public class DevMenuOverlayTests
    {
        private GameObject _overlayGo;
        private GameObject _commandsGo;
        private DevMenuOverlay _overlay;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _commandsGo = new GameObject("OverlayTestCommands");
            _commandsGo.AddComponent<OverlayTestCommands>();
            _overlayGo = new GameObject("TestOverlay");
            _overlay = _overlayGo.AddComponent<DevMenuOverlay>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(_overlayGo);
            Object.Destroy(_commandsGo);
            yield return null;
        }

        private VisualElement Root => _overlay.GetComponentInChildren<UIDocument>().rootVisualElement;
        private VisualElement Panel => Root.Q<VisualElement>("devmenu-overlay");
        private Foldout Group(string name) => Root.Query<Foldout>().ToList().First(f => f.text == name);

        [UnityTest]
        public IEnumerator Open_ShowsPanelAndGroupedCommands()
        {
            _overlay.Open();
            yield return null;

            Assert.IsTrue(_overlay.IsOpen);
            Assert.IsTrue(DevMenuOverlayInput.IsOpen);
            Assert.AreEqual(DisplayStyle.Flex, Panel.resolvedStyle.display);
            Assert.IsNotNull(Group("OverlayTests"));
        }

        [UnityTest]
        public IEnumerator Close_HidesPanelAndClearsInputState()
        {
            _overlay.Open();
            yield return null;
            _overlay.Close();
            yield return null;

            Assert.IsFalse(_overlay.IsOpen);
            Assert.IsFalse(DevMenuOverlayInput.IsOpen);
            Assert.IsFalse(DevMenuOverlayInput.IsPointerOverOverlay);
            Assert.AreEqual(DisplayStyle.None, Panel.resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator Toggle_FlipsOpenState()
        {
            _overlay.Toggle();
            yield return null;
            Assert.IsTrue(_overlay.IsOpen);
            _overlay.Toggle();
            yield return null;
            Assert.IsFalse(_overlay.IsOpen);
        }

        [UnityTest]
        public IEnumerator LogLines_AreCapturedBeforeFirstOpen()
        {
            Debug.Log("overlay test line");
            yield return null;

            Assert.AreEqual("overlay test line", _overlay.LogEntries.Last().Message);
        }

        [UnityTest]
        public IEnumerator CollapsedGroup_StaysCollapsedAcrossCloseAndReopen()
        {
            _overlay.Open();
            yield return null;
            Group("OverlayTests").value = false;
            _overlay.Close();
            _overlay.Open();
            yield return null;

            Assert.IsFalse(Group("OverlayTests").value);
        }

        [UnityTest]
        public IEnumerator ChangingBoolParameter_DoesNotCollapseGroup()
        {
            _overlay.Open();
            yield return null;
            var group = Group("OverlayTests");
            var toggle = group.Query<Toggle>().ToList().First(t => t != group.Q<Toggle>(className: Foldout.toggleUssClassName));
            toggle.value = !toggle.value;
            yield return null;

            Assert.IsTrue(group.value);
        }
    }
}
