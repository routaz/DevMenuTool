using System.Collections.Generic;
using DevMenuTool.Core;
using DevMenuTool.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevMenuTool.UI
{
    /// <summary>
    /// Runtime DevMenu panel for Play Mode and development builds: toggled with <see cref="ToggleKey"/>,
    /// lists [DevMenu] commands grouped by name, and shows the last log lines. The game keeps running.
    /// Spawned by <see cref="DevMenuPlayModeInjector"/>. Only one overlay is active: a second one
    /// logs a warning and destroys its own component.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class DevMenuOverlay : MonoBehaviour
    {
        public const int SortingOrder = 10000;

        [SerializeField] private KeyCode toggleKey = KeyCode.F1;

        private readonly DevMenuLogBuffer _log = new();
        private readonly HashSet<string> _collapsedGroups = new();
        private int _renderedLogVersion = -1;

        private PanelSettings _panelSettings;
        private UIDocument _document;
        private VisualElement _panel;
        private ScrollView _commands;
        private VisualElement _logStrip;
        private Label _hint;
        private UIBuilder _uiBuilder;

        public static DevMenuOverlay Instance { get; private set; }

        public bool IsOpen { get; private set; }

        public KeyCode ToggleKey
        {
            get => toggleKey;
            set => toggleKey = value;
        }

        public IReadOnlyList<DevMenuLogEntry> LogEntries => _log.Snapshot();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[DevMenu] A DevMenuOverlay already exists on '{Instance.name}'; destroying the duplicate on '{name}'.");
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            if (Instance == this)
                Application.logMessageReceived += OnLogMessage;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessage;
            if (Instance == this)
                DevMenuOverlayInput.Clear();
        }

        private void OnDestroy()
        {
            if (_panelSettings != null)
                Destroy(_panelSettings);
            if (Instance == this)
                Instance = null;
        }

        private void OnLogMessage(string message, string stackTrace, LogType type) => _log.Add(type, message);

        private void Update()
        {
            if (Instance != this) return;

            if (DevMenuInput.WasPressedThisFrame(toggleKey))
                Toggle();

            if (IsOpen)
                RenderLog(force: false);

            PublishInputState();
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            EnsureBuilt();
            _hint.text = $"{DevMenuInput.EffectiveToggleKey(toggleKey)} to close";
            RebuildCommands();
            _panel.style.display = DisplayStyle.Flex;
            IsOpen = true;
            RenderLog(force: true);
            PublishInputState();
        }

        public void Close()
        {
            (_panel?.panel?.focusController?.focusedElement as VisualElement)?.Blur();
            if (_panel != null)
                _panel.style.display = DisplayStyle.None;
            IsOpen = false;
            PublishInputState();
        }

        private void EnsureBuilt()
        {
            if (_document != null) return;

            _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            _panelSettings.name = "DevMenuOverlayPanelSettings";
            _panelSettings.themeStyleSheet = Resources.Load<ThemeStyleSheet>("DevMenuRuntimeTheme");
            _panelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
            _panelSettings.scale = 1f;
            _panelSettings.sortingOrder = SortingOrder;

            // Host the UIDocument on an inactive child so OnEnable runs with the panel settings already set.
            var host = new GameObject("DevMenuOverlayDocument");
            host.SetActive(false);
            host.transform.SetParent(transform, false);
            _document = host.AddComponent<UIDocument>();
            _document.panelSettings = _panelSettings;
            host.SetActive(true);

            var root = _document.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;
            root.styleSheets.Add(Resources.Load<StyleSheet>("DevMenu"));
            root.styleSheets.Add(Resources.Load<StyleSheet>("DevMenuOverlay"));

            var tree = Resources.Load<VisualTreeAsset>("DevMenuOverlay").CloneTree();
            tree.pickingMode = PickingMode.Ignore;
            tree.style.position = Position.Absolute;
            tree.style.left = 0;
            tree.style.right = 0;
            tree.style.top = 0;
            tree.style.bottom = 0;
            root.Add(tree);

            _panel = root.Q<VisualElement>("devmenu-overlay");
            _commands = root.Q<ScrollView>("devmenu-commands");
            _logStrip = root.Q<VisualElement>("devmenu-log");
            _hint = root.Q<Label>("devmenu-hint");
            _panel.style.display = DisplayStyle.None;

            _uiBuilder = new UIBuilder(DevMenuContainer.MethodInvoker, DevMenuContainer.ParameterHandlerRegistry);
        }

        private void RebuildCommands()
        {
            var scanner = DevMenuContainer.MethodScanner;
            scanner.ScanScene();
            _commands.Clear();

            foreach (var group in DevMenuCommandGroups.Build(scanner.CachedMethods))
            {
                var foldout = new Foldout { text = group.Name, value = !_collapsedGroups.Contains(group.Name) };
                foldout.AddToClassList("devmenu-overlay__group");

                string groupName = group.Name;
                foldout.RegisterValueChangedCallback(evt =>
                {
                    // Parameter fields inside the group raise bool change events that bubble up here.
                    if (evt.target != foldout) return;
                    if (evt.newValue) _collapsedGroups.Remove(groupName);
                    else _collapsedGroups.Add(groupName);
                });

                foreach (var method in group.Methods)
                    foldout.Add(_uiBuilder.BuildMethodUI(method));

                _commands.Add(foldout);
            }
        }

        private void RenderLog(bool force)
        {
            if (_logStrip == null || (!force && _renderedLogVersion == _log.Version)) return;

            _renderedLogVersion = _log.Version;
            _logStrip.Clear();
            foreach (var entry in _log.Snapshot())
            {
                var line = new Label(entry.Message);
                line.AddToClassList("devmenu-log__line");
                if (entry.Type == LogType.Warning)
                    line.AddToClassList("devmenu-log--warning");
                else if (entry.Type is LogType.Error or LogType.Exception or LogType.Assert)
                    line.AddToClassList("devmenu-log--error");
                _logStrip.Add(line);
            }
        }

        private void PublishInputState()
        {
            if (Instance != this) return;
            DevMenuOverlayInput.Set(IsOpen, IsOpen && IsPointerOverPanel(), IsOpen && IsTextInputFocused());
        }

        private bool IsPointerOverPanel()
        {
            if (_panel?.panel == null || !DevMenuInput.TryGetPointerPosition(out var screen)) return false;
            var panelPosition = RuntimePanelUtils.ScreenToPanel(_panel.panel, new Vector2(screen.x, Screen.height - screen.y));
            return _panel.worldBound.Contains(panelPosition);
        }

        private bool IsTextInputFocused()
        {
            for (var element = _panel?.panel?.focusController?.focusedElement as VisualElement; element != null; element = element.parent)
            {
                if (element is TextField || element is IntegerField || element is FloatField
                    || element is LongField || element is DoubleField)
                    return true;
            }
            return false;
        }
    }
}
