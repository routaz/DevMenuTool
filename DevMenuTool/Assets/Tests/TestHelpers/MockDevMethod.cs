using System;
using System.Collections.Generic;
using System.Reflection;
using DevMenuTool.Core.Interfaces;
using UnityEngine;

namespace DevMenuTool.Tests.TestHelpers
{
    public class MockDevMethod : IDevMethod
    {
        public string GroupName { get; set; }
        public string ButtonLabel { get; set; }
        public KeyCode ShortcutKey { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public object TargetInstance { get; set; }
        public ParameterInfo[] Parameters { get; set; }

        private readonly Dictionary<int, object> _parameters = new Dictionary<int, object>();
        private readonly Dictionary<int, List<IParameterValidator>> _validators = new Dictionary<int, List<IParameterValidator>>();
        private readonly List<IParameterChangeHandler> _changeHandlers = new List<IParameterChangeHandler>();

        public MockDevMethod(string buttonLabel = "MockMethod", string groupName = "MockGroup", KeyCode shortcutKey = KeyCode.None)
        {
            ButtonLabel = buttonLabel;
            GroupName = groupName;
            ShortcutKey = shortcutKey;
            Parameters = new ParameterInfo[0];
        }

        public MockDevMethod(string buttonLabel, object targetInstance, MethodInfo methodInfo, ParameterInfo[] parameters = null)
        {
            ButtonLabel = buttonLabel;
            TargetInstance = targetInstance;
            MethodInfo = methodInfo;
            Parameters = parameters ?? new ParameterInfo[0];
            GroupName = "MockGroup";
            ShortcutKey = KeyCode.None;
        }

        public void Invoke(object[] parameters = null)
        {
            if (MethodInfo != null && TargetInstance != null)
            {
                MethodInfo.Invoke(TargetInstance, parameters ?? new object[0]);
            }
        }

        public static MockDevMethod CreateWithParameters(string buttonLabel, object targetInstance, Type[] parameterTypes, string[] parameterNames = null)
        {
            var parameters = new ParameterInfo[parameterTypes.Length];
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                parameters[i] = new MockParameterInfo(parameterNames?[i] ?? $"param{i}", parameterTypes[i]);
            }

            return new MockDevMethod(buttonLabel, targetInstance, null, parameters);
        }

        private class MockParameterInfo : ParameterInfo
        {
            private readonly string _name;
            private readonly Type _parameterType;
            private readonly bool _hasDefaultValue;
            private readonly object _defaultValue;

            public MockParameterInfo(string name, Type parameterType, bool hasDefaultValue = false, object defaultValue = null)
            {
                _name = name;
                _parameterType = parameterType;
                _hasDefaultValue = hasDefaultValue;
                _defaultValue = defaultValue;
            }

            public override string Name => _name;
            public override Type ParameterType => _parameterType;
            public override bool HasDefaultValue => _hasDefaultValue;
            public override object DefaultValue => _defaultValue;
        }

        public object[] GetCurrentParameters()
        {
            var result = new object[Parameters.Length];
            for (int i = 0; i < Parameters.Length; i++)
            {
                result[i] = _parameters.TryGetValue(i, out var value) ? value : Parameters[i].HasDefaultValue ? Parameters[i].DefaultValue : null;
            }
            return result;
        }

        public void SetParameter(int index, object value)
        {
            if (index < 0 || index >= Parameters.Length)
                return;

            var oldValue = _parameters.TryGetValue(index, out var existingValue) ? existingValue : Parameters[index].HasDefaultValue ? Parameters[index].DefaultValue : null;
            
            if (_validators.TryGetValue(index, out var validators))
            {
                foreach (var validator in validators)
                {
                    var result = validator.Validate(Parameters[index].Name, value, Parameters[index].ParameterType);
                    if (!result.IsValid)
                    {
                        Debug.LogWarning($"Parameter validation failed: {result.ErrorMessage}");
                        return;
                    }
                }
            }

            _parameters[index] = value;
            
            ParameterChanged?.Invoke(this, new ParameterChangeEventArgs(
                ButtonLabel, 
                Parameters[index].Name, 
                oldValue, 
                value, 
                Parameters[index].ParameterType));
        }

        public void ResetToDefaults()
        {
            for (int i = 0; i < Parameters.Length; i++)
            {
                var defaultValue = Parameters[i].HasDefaultValue ? Parameters[i].DefaultValue : null;
                SetParameter(i, defaultValue);
            }
        }

        public void SaveToEditorPrefs()
        {
            Debug.Log($"Mock: Saving parameters for method {ButtonLabel}");
        }

        public bool LoadFromEditorPrefs()
        {
            Debug.Log($"Mock: Loading parameters for method {ButtonLabel}");
            return true;
        }

        public void AddValidator(int parameterIndex, IParameterValidator validator)
        {
            if (!_validators.ContainsKey(parameterIndex))
                _validators[parameterIndex] = new List<IParameterValidator>();
            
            _validators[parameterIndex].Add(validator);
        }

        public void RemoveValidator(int parameterIndex, IParameterValidator validator)
        {
            if (_validators.TryGetValue(parameterIndex, out var validators))
            {
                validators.Remove(validator);
            }
        }

        public void ClearValidators(int parameterIndex)
        {
            _validators.Remove(parameterIndex);
        }

        public ParameterValidationResult ValidateParameter(int parameterIndex, object value)
        {
            if (parameterIndex < 0 || parameterIndex >= Parameters.Length)
                return ParameterValidationResult.Failure($"Invalid parameter index: {parameterIndex}");

            if (!_validators.TryGetValue(parameterIndex, out var validators))
                return ParameterValidationResult.Success();

            foreach (var validator in validators)
            {
                var result = validator.Validate(Parameters[parameterIndex].Name, value, Parameters[parameterIndex].ParameterType);
                if (!result.IsValid)
                    return result;
            }

            return ParameterValidationResult.Success();
        }

        public event EventHandler<ParameterChangeEventArgs> ParameterChanged;
        
        public void AddParameterChangeHandler(IParameterChangeHandler handler)
        {
            if (handler != null && !_changeHandlers.Contains(handler))
            {
                _changeHandlers.Add(handler);
            }
        }

        public void RemoveParameterChangeHandler(IParameterChangeHandler handler)
        {
            _changeHandlers.Remove(handler);
        }
    }
} 