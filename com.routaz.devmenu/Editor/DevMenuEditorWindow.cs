using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
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

        [MenuItem("DevMenu/Open Dev Menu")]
        public static void ShowWindow()
        {
            var window = GetWindow<DevMenuEditorWindow>();
            window.titleContent = new GUIContent("Dev Menu");
            window.minSize = new Vector2(300, 200);
        }

        private void OnEnable()
        {
            // Initialize dependencies when the window is enabled.
            // This is a better place than CreateGUI for setting up instances.
            _methodScanner = DevMenuContainer.MethodScanner;
            _uiBuilder = new UIBuilder(DevMenuContainer.MethodInvoker);
        }

        public void CreateGUI()
        {
            // The CreateGUI method is now a clean entry point.
            BuildWindowUI();
        }

        private void BuildWindowUI()
        {
            var root = rootVisualElement;
            root.Clear();

            LoadStyles(root);

            var scrollView = CreateScrollView(root);

            PopulateMethods(scrollView);
        }

        private void LoadStyles(VisualElement root)
        {
            var styleSheet = Resources.Load<StyleSheet>("DevMenu"); //
            if (styleSheet != null)
                root.styleSheets.Add(styleSheet); //
        }

        private ScrollView CreateScrollView(VisualElement root)
        {
            var scrollView = new ScrollView(); //
            scrollView.name = "method-scroll"; //
            scrollView.AddToClassList("scroll-container"); //
            root.Add(scrollView); //
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
            var groupContainer = new VisualElement(); //
            groupContainer.AddToClassList("group-container"); //
            scrollView.Add(groupContainer); //
            return groupContainer;
        }

        private void BuildMethodGroupUI(VisualElement container, IGrouping<string, IDevMethod> group)
        {
            var header = new Label(group.Key); //
            header.AddToClassList("group-header"); //
            container.Add(header); //

            foreach (var method in group)
            {
                // Use the UIBuilder instance to create the UI for each method.
                var methodUI = _uiBuilder.BuildMethodUI(method);
                container.Add(methodUI); //
            }
        }
    }
}