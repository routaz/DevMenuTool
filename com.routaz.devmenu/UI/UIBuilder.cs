using System.Collections.Generic;
using System.Reflection;
using DevMenuTool.Core;
using UnityEngine;
using UnityEngine.UIElements;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Runtime;
using DevMenuTool.Runtime.Interfaces;

#if UNITY_EDITOR
using UnityEditor;
using DevMenuTool.Editor;
#endif

namespace DevMenuTool.UI
{
    public class UIBuilder
    {
        private readonly IMethodInvoker _invoker;
        private readonly IParameterHandlerRegistry _registry;

        public UIBuilder(IMethodInvoker invoker, IParameterHandlerRegistry registry)
        {
            _invoker = invoker;
            _registry = registry;
        }

        public VisualElement BuildMethodUI(IDevMethod method)
        {
            VisualElement root = CreateRootElement();
            if (root == null)
            {
                return new Label("Missing UI Template");
            }

            var body = root.Q<VisualElement>("method-body");
            
            SetupHeader(root, method);
            var parameterFields = CreateParameterFields(body, method);
            SetupInvokeButton(body, method, parameterFields);
            SetupResetButton(body, method, parameterFields);

            return root;
        }

        private VisualElement CreateRootElement()
        {
            var template = Resources.Load<VisualTreeAsset>("MethodContainer");
            var styleSheet = Resources.Load<StyleSheet>("DevMenu");

            if (template == null || styleSheet == null)
            {
                Debug.LogError("Missing MethodContainer.uxml or DevMenu.uss from Resources.");
                return null;
            }

            var root = template.CloneTree();
            root.styleSheets.Add(styleSheet);
            return root;
        }

        private void SetupHeader(VisualElement root, IDevMethod method)
        {
            var titleLabel = root.Q<Label>("method-title");
            var defaultKey = method.ShortcutKey;
            var methodSignature = method.MethodInfo.ToString();

            #if UNITY_EDITOR
                var actualKey = DevMenuKeybinds.GetKey(methodSignature, defaultKey);
            #else
                var actualKey = defaultKey;
            #endif

            titleLabel.text = $"{method.MethodInfo.Name} ({actualKey})";
        }

        private List<(VisualElement field, ParameterInfo info)> CreateParameterFields(VisualElement body, IDevMethod method)
        {
            var parameterFields = new List<(VisualElement field, ParameterInfo info)>();
            var methodSignature = method.MethodInfo.ToString();
            var currentParams = method.GetCurrentParameters();

            for (int i = 0; i < method.Parameters.Length; i++)
            {
                var param = method.Parameters[i];
                var currentValue = i < currentParams.Length ? currentParams[i] : null;
                var field = CreateFieldForParameter(param, methodSignature, currentValue, method, i);
                field.style.marginBottom = 2;
                parameterFields.Add((field, param));
                body.Add(field);
            }

            return parameterFields;
        }

        private VisualElement CreateFieldForParameter(ParameterInfo param, string methodSignature, object currentValue, IDevMethod method, int parameterIndex)
        {
            var tooltip = $"Parameter: {param.Name} ({param.ParameterType.Name})";
            if (param.HasDefaultValue)
                tooltip += $" (Default: {param.DefaultValue ?? "null"})";

            VisualElement field = param.ParameterType switch
            {
                var t when t == typeof(int) => new IntegerField(param.Name) { value = currentValue != null ? (int)currentValue : (param.HasDefaultValue ? (int)param.DefaultValue : default) },
                var t when t == typeof(float) => new FloatField(param.Name) { value = currentValue != null ? (float)currentValue : (param.HasDefaultValue ? (float)param.DefaultValue : default) },
                var t when t == typeof(bool) => new Toggle(param.Name) { value = currentValue != null ? (bool)currentValue : (param.HasDefaultValue ? (bool)param.DefaultValue : default) },
                var t when t.IsEnum => new EnumField(param.Name, (System.Enum)(currentValue ?? (param.HasDefaultValue ? param.DefaultValue : System.Activator.CreateInstance(t)))),
                _ => new TextField(param.Name) { value = currentValue?.ToString() ?? (param.HasDefaultValue && param.DefaultValue != null ? param.DefaultValue.ToString() : "") }
            };

            field.tooltip = tooltip;
            
            void Persist(object newValue)
            {
                method.SetParameter(parameterIndex, newValue);
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    var stringValue = _registry.ToString(newValue);
                    DevMenuParameterPrefs.SetValue(methodSignature, param.Name, stringValue);
                }
                #endif
            }

            switch (field)
            {
                case INotifyValueChanged<string> f: f.RegisterValueChangedCallback(evt => Persist(evt.newValue)); break;
                case INotifyValueChanged<int> f: f.RegisterValueChangedCallback(evt => Persist(evt.newValue)); break;
                case INotifyValueChanged<float> f: f.RegisterValueChangedCallback(evt => Persist(evt.newValue)); break;
                case INotifyValueChanged<bool> f: f.RegisterValueChangedCallback(evt => Persist(evt.newValue)); break;
                case INotifyValueChanged<System.Enum> f: f.RegisterValueChangedCallback(evt => Persist(evt.newValue)); break;
            }

            return field;
        }

        private void SetupInvokeButton(VisualElement body, IDevMethod method, List<(VisualElement field, ParameterInfo info)> parameterFields)
        {
            var button = new Button { text = "Run" };

            if (Application.isPlaying)
            {
                button.clicked += () =>
                {
                    var args = GetArgumentsFromFields(parameterFields);
                    _invoker.Invoke(method, args);
                };
            }
            else
            {
                button.SetEnabled(false);
            }

            #if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    var methodSignature = method.MethodInfo.ToString();
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.SpaceBetween, marginTop = 4 } };
                    button.style.flexGrow = 1;
                    button.style.marginRight = 6;
                    
                    var keyField = new EnumField(DevMenuKeybinds.GetKey(methodSignature, method.ShortcutKey)) { style = { minWidth = 80, flexShrink = 0 } };
                    keyField.RegisterValueChangedCallback(evt =>
                    {
                        var newKey = (KeyCode)evt.newValue;
                        DevMenuKeybinds.SetKey(methodSignature, newKey);
                        body.parent.Q<Label>("method-title").text = $"{method.MethodInfo.Name} ({newKey})";
                    });

                    row.Add(button);
                    row.Add(keyField);
                    body.Add(row);
                }
                else
                {
                    body.Add(button);
                }
            #else
                body.Add(button);
#endif
        }

        private void SetupResetButton(VisualElement body, IDevMethod method, List<(VisualElement field, ParameterInfo info)> parameterFields)
        {
            if (method.Parameters.Length == 0) return; // No parameters to reset

            var resetButton = new Button { text = "Reset to Defaults" };
            resetButton.style.marginTop = 4;
            resetButton.style.marginBottom = 4;

            resetButton.clicked += () =>
            {
                method.ResetToDefaults();
                
                RefreshParameterFields(method, parameterFields);
            };

            body.Add(resetButton);
        }

        private void RefreshParameterFields(IDevMethod method, List<(VisualElement field, ParameterInfo info)> parameterFields)
        {
            var currentParams = method.GetCurrentParameters();
            
            for (int i = 0; i < parameterFields.Count && i < currentParams.Length; i++)
            {
                var (field, paramInfo) = parameterFields[i];
                var newValue = currentParams[i];

                switch (field)
                {
                    case IntegerField f: f.value = newValue != null ? (int)newValue : (paramInfo.HasDefaultValue ? (int)paramInfo.DefaultValue : default); break;
                    case FloatField f: f.value = newValue != null ? (float)newValue : (paramInfo.HasDefaultValue ? (float)paramInfo.DefaultValue : default); break;
                    case Toggle f: f.value = newValue != null ? (bool)newValue : (paramInfo.HasDefaultValue ? (bool)paramInfo.DefaultValue : default); break;
                    case EnumField f: f.value = (System.Enum)(newValue ?? (paramInfo.HasDefaultValue ? paramInfo.DefaultValue : System.Activator.CreateInstance(paramInfo.ParameterType))); break;
                    case TextField f: f.value = newValue?.ToString() ?? (paramInfo.HasDefaultValue && paramInfo.DefaultValue != null ? paramInfo.DefaultValue.ToString() : ""); break;
                }
            }
        }

        private object[] GetArgumentsFromFields(List<(VisualElement field, ParameterInfo info)> parameterFields)
        {
            var args = new object[parameterFields.Count];
            for (int i = 0; i < parameterFields.Count; i++)
            {
                var (element, paramInfo) = parameterFields[i];
                object value = null;

                switch (element)
                {
                    case IntegerField f: value = f.value; break;
                    case FloatField f: value = f.value; break;
                    case Toggle f: value = f.value; break;
                    case EnumField f: value = f.value; break;
                    case TextField f when !string.IsNullOrWhiteSpace(f.value):
                        try { value = _registry.Parse(f.value, paramInfo.ParameterType); } catch { value = null; }
                        break;
                }
                args[i] = value;
            }
            return args;
        }
    }
}