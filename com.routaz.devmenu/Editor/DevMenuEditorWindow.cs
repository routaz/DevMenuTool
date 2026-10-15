using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using DevMenuTool.Core;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Runtime;
using DevMenuTool.UI;

namespace DevMenuTool.Editor
{
    public class DevMenuEditorWindow : EditorWindow
    {
        private UIBuilder _uiBuilder;
        private IMethodScanner _methodScanner;
        private IDevMenuMethodIndex _methodIndex;

        [MenuItem("DevMenu/Open Dev Menu")]
        public static void ShowWindow()
        {
            var window = GetWindow<DevMenuEditorWindow>();
            window.titleContent = new GUIContent("Dev Menu");
            window.minSize = new Vector2(300, 200);
        }

        private void OnEnable()
        {
            _methodScanner = DevMenuContainer.MethodScanner;
            _uiBuilder = new UIBuilder(DevMenuContainer.MethodInvoker, DevMenuContainer.ParameterHandlerRegistry);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            _methodIndex = DevMenuContainer.MethodIndex;
            _methodIndex.Rebuilt += BuildWindowUI;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            _methodIndex.Rebuilt -= BuildWindowUI;
        }

        // CreateGUI also runs after every domain reload, so this covers script recompiles.
        public void CreateGUI()
        {
            BuildWindowUI();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // Rescan once the new mode is fully entered: scene objects differ between modes,
            // and Run buttons are only enabled while playing.
            if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.EnteredEditMode)
                BuildWindowUI();
        }

        private void BuildWindowUI()
        {
            var root = rootVisualElement;
            root.Clear();

            LoadStyles(root);

            root.Add(CreateToolbar());

            var scrollView = CreateScrollView(root);

            PopulateMethods(scrollView);
        }

        private Toolbar CreateToolbar()
        {
            var toolbar = new Toolbar();
            toolbar.Add(new ToolbarButton(BuildWindowUI) { text = "Refresh", tooltip = "Rescan loaded scenes for [DevMenu] methods" });
            return toolbar;
        }

        private void LoadStyles(VisualElement root)
        {
            var styleSheet = Resources.Load<StyleSheet>("DevMenu");
            if (styleSheet != null)
                root.styleSheets.Add(styleSheet);
        }

        private ScrollView CreateScrollView(VisualElement root)
        {
            var scrollView = new ScrollView();
            scrollView.name = "method-scroll";
            scrollView.AddToClassList("scroll-container");
            root.Add(scrollView);
            return scrollView;
        }

        private void PopulateMethods(ScrollView scrollView)
        {
            _methodScanner.ScanScene();

            var groupedMethods = _methodScanner.CachedMethods
                .GroupBy(m => m.GroupName)
                .OrderBy(g => g.Key);

            foreach (var group in groupedMethods)
            {
                var groupContainer = CreateGroupContainer(scrollView);
                BuildMethodGroupUI(groupContainer, group);
            }
        }

        private VisualElement CreateGroupContainer(ScrollView scrollView)
        {
            var groupContainer = new VisualElement();
            groupContainer.AddToClassList("group-container");
            scrollView.Add(groupContainer);
            return groupContainer;
        }

        private void BuildMethodGroupUI(VisualElement container, IGrouping<string, IDevMethod> group)
        {
            var header = new Label(group.Key);
            header.AddToClassList("group-header");
            container.Add(header);

            foreach (var method in group)
            {
                var methodUI = _uiBuilder.BuildMethodUI(method);
                container.Add(methodUI);
            }
        }
    }
}