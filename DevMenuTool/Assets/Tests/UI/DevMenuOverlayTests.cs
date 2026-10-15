using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using DevMenuTool.Core.Attributes;
using DevMenuTool.Runtime;
using DevMenuTool.UI;

namespace DevMenuTool.Tests.UI
{
    public class OverlayTestCommands : MonoBehaviour
    {
        public bool Flag;

        [DevMenu("OverlayTests")]
        private void SetFlag(bool flag = false) => Flag = flag;

        [DevMenu("OverlayTests")]
        private void SetName(string name = "") { }
    }

    public class DevMenuOverlayTests
    {
        private GameObject _overlayGo;
        private GameObject _commandsGo;
        private DevMenuOverlay _overlay;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            foreach (var existing in Object.FindObjectsByType<DevMenuOverlay>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Object.Destroy(existing.gameObject);
            yield return null;

            _commandsGo = new GameObject("OverlayTestCommands");
            _commandsGo.AddComponent<OverlayTestCommands>();
            _overlayGo = new GameObject("TestOverlay");
            _overlay = _overlayGo.AddComponent<DevMenuOverlay>();
            yield return null;
            Assert.AreSame(_overlay, DevMenuOverlay.Instance);
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
            toggle.value = true;
            yield return null;
            toggle.value = false; // a newValue == false change bubbles up to the foldout
            yield return null;
            _overlay.Close();
            _overlay.Open();
            yield return null;

            Assert.IsTrue(Group("OverlayTests").value);
        }

        [UnityTest]
        public IEnumerator SecondOverlay_IsDestroyedAndInstanceUnchanged()
        {
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("DevMenuOverlay already exists"));
            var duplicateGo = new GameObject("DuplicateOverlay");
            var duplicate = duplicateGo.AddComponent<DevMenuOverlay>();
            yield return null;

            Assert.AreSame(_overlay, DevMenuOverlay.Instance);
            Assert.IsTrue(duplicate == null, "the duplicate's component should be destroyed");
            Object.Destroy(duplicateGo);
        }

        [UnityTest]
        public IEnumerator Close_BlursFocusedTextField()
        {
            _overlay.Open();
            yield return null;
            var field = Group("OverlayTests").Q<TextField>();
            field.Focus();
            yield return null;
            Assert.IsNotNull(Panel.panel.focusController.focusedElement, "precondition: the field has focus");

            _overlay.Close();

            Assert.IsNull(Panel.panel.focusController.focusedElement);
        }

        [Test]
        public void EffectiveToggleKey_LegacyInput_ReturnsRequestedKey()
        {
            Assert.AreEqual(KeyCode.K, DevMenuInput.EffectiveToggleKey(KeyCode.K));
            Assert.AreEqual(KeyCode.F2, DevMenuInput.EffectiveToggleKey(KeyCode.F2));
        }
    }
}
