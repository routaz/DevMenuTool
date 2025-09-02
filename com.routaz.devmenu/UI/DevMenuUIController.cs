using UnityEngine;
using UnityEngine.UIElements;
using DevMenuTool.Core;
using DevMenuTool.Runtime;

namespace DevMenuTool.UI
{
    public class DevMenuUIController : MonoBehaviour
    {
        public UIDocument uiDocument;

        private UIBuilder _uiBuilder;
        private IMethodScanner _methodScanner;

        private void Start()
        {
            _methodScanner = DevMenuContainer.MethodScanner;
            _uiBuilder = new UIBuilder(DevMenuContainer.MethodInvoker);

            if (!TryGetScrollView(out var scrollView))
            {
                return;
            }

            PopulateMethods(scrollView);
        }

        private bool TryGetScrollView(out ScrollView scrollView)
        {
            scrollView = null;

            if (uiDocument == null)
            {
                Debug.LogError("UIDocument not assigned to DevMenuUIController.");
                return false;
            }

            var root = uiDocument.rootVisualElement;
            scrollView = root.Q<ScrollView>("method-scroll");

            if (scrollView == null)
            {
                Debug.LogError("ScrollView with name 'method-scroll' not found in UXML.");
                return false;
            }

            return true;
        }

        private void PopulateMethods(ScrollView scrollView)
        {
            _methodScanner.ScanScene();

            foreach (var method in _methodScanner.CachedMethods)
            {
                // Use the UIBuilder instance to create the UI for each method
                var methodUI = _uiBuilder.BuildMethodUI(method);
                scrollView.Add(methodUI);
            }
        }
    }
}