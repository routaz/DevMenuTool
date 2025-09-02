// DevMethod.cs
using System;
using System.Reflection;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Core.Attributes;
using UnityEngine;

namespace DevMenuTool.Core
{
    /// <summary>
    /// Represents a dev menu method with its metadata and execution capabilities.
    /// This class focuses on method information and invocation, delegating other responsibilities.
    /// </summary>
    public class DevMethod : IDevMethod
    {
        public string GroupName { get; }
        public string ButtonLabel => MethodInfo.Name;
        public KeyCode ShortcutKey { get; }

        public MethodInfo MethodInfo { get; }
        public object TargetInstance { get; }
        public ParameterInfo[] Parameters => MethodInfo.GetParameters();

        // Delegated responsibilities
        private readonly IParameterManager _parameterManager;
        private readonly IParameterValidator _parameterValidator;
        private readonly IParameterEventManager _eventManager;

        public DevMethod(MethodInfo methodInfo, object targetInstance, DevMenuAttribute attribute)
        {
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            TargetInstance = targetInstance ?? throw new ArgumentNullException(nameof(targetInstance));
            GroupName = attribute?.GroupName ?? string.Empty;
            ShortcutKey = attribute?.ShortcutKey ?? KeyCode.None;
            
            // Initialize delegated components
            _parameterManager = new ParameterManager(this);
            _parameterValidator = new ParameterValidator(this);
            _eventManager = new ParameterEventManager(this);
        }

        // Parameter management delegation
        public object[] GetCurrentParameters() => _parameterManager.GetCurrentParameters();
        
        public void SetParameter(int index, object value)
        {
            // Validate the new value first
            var validationResult = _parameterValidator.ValidateParameter(index, value);
            if (!validationResult.IsValid)
            {
                Debug.LogWarning($"Parameter validation failed: {validationResult.ErrorMessage}");
                // When validation fails, set to default value instead of keeping current value
                var paramInfo = Parameters[index];
                value = paramInfo.HasDefaultValue 
                    ? paramInfo.DefaultValue 
                    : (paramInfo.ParameterType.IsValueType ? Activator.CreateInstance(paramInfo.ParameterType) : null);
            }

            // Get old value for change event
            var oldValue = _parameterManager.GetCurrentParameters()[index];
            
            // Update the parameter
            _parameterManager.SetParameter(index, value);
            
            // Trigger change event
            _eventManager.OnParameterChanged(index, oldValue, value);
        }
        
        public void ResetToDefaults()
        {
            var oldParameters = _parameterManager.GetCurrentParameters();
            _parameterManager.ResetToDefaults();
            
            // Trigger change events for all parameters
            var newParameters = _parameterManager.GetCurrentParameters();
            for (int i = 0; i < Parameters.Length; i++)
            {
                _eventManager.OnParameterChanged(i, oldParameters[i], newParameters[i]);
            }
        }
        
        public void SaveToEditorPrefs() => _parameterManager.SaveToEditorPrefs();
        public bool LoadFromEditorPrefs() => _parameterManager.LoadFromEditorPrefs();

        // Validation delegation
        public void AddValidator(int parameterIndex, IParameterValidator validator) => 
            _parameterValidator.AddValidator(parameterIndex, validator);
        public void RemoveValidator(int parameterIndex, IParameterValidator validator) => 
            _parameterValidator.RemoveValidator(parameterIndex, validator);
        public void ClearValidators(int parameterIndex) => 
            _parameterValidator.ClearValidators(parameterIndex);
        public ParameterValidationResult ValidateParameter(int parameterIndex, object value) => 
            _parameterValidator.ValidateParameter(parameterIndex, value);

        // Event management delegation
        public event EventHandler<ParameterChangeEventArgs> ParameterChanged
        {
            add => _eventManager.ParameterChanged += value;
            remove => _eventManager.ParameterChanged -= value;
        }
        public void AddParameterChangeHandler(IParameterChangeHandler handler) => 
            _eventManager.AddParameterChangeHandler(handler);
        public void RemoveParameterChangeHandler(IParameterChangeHandler handler) => 
            _eventManager.RemoveParameterChangeHandler(handler);

        /// <summary>
        /// Provides a direct, simple invocation of the method.
        /// Uses stored parameters if none are supplied.
        /// </summary>
        public void Invoke(object[] parameters = null)
        {
            var argsToUse = parameters ?? GetCurrentParameters();
            MethodInfo.Invoke(TargetInstance, argsToUse);
        }
    }
}