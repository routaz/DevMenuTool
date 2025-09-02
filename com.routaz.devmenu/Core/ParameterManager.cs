using System;
using System.Reflection;
using DevMenuTool.Core.Interfaces;
using UnityEngine;

#if UNITY_EDITOR
using DevMenuTool.Editor;
#endif

namespace DevMenuTool.Core
{
    /// <summary>
    /// Manages parameter storage, persistence, and state for a DevMethod.
    /// </summary>
    public class ParameterManager : IParameterManager
    {
        private readonly IDevMethod _devMethod;
        private object[] _currentParameters;
        private bool _parametersInitialized = false;

        public ParameterManager(IDevMethod devMethod)
        {
            _devMethod = devMethod ?? throw new ArgumentNullException(nameof(devMethod));
            _currentParameters = new object[_devMethod.Parameters.Length];
            InitializeParameters();
        }

        private void InitializeParameters()
        {
            if (_parametersInitialized) return;
            
            // Try to load from EditorPrefs first
            if (LoadFromEditorPrefs())
            {
                _parametersInitialized = true;
                return;
            }
            
            // Fall back to default values
            ResetToDefaults();
            _parametersInitialized = true;
        }

        public object[] GetCurrentParameters()
        {
            InitializeParameters();
            return (object[])_currentParameters.Clone();
        }

        public void SetParameter(int index, object value)
        {
            if (index < 0 || index >= _currentParameters.Length)
                return;

            // Store old value for change event
            var oldValue = _currentParameters[index];
            
            // Update the parameter
            _currentParameters[index] = value;
            
            // Save to EditorPrefs
            SaveToEditorPrefs();
        }

        public void ResetToDefaults()
        {
            for (int i = 0; i < _devMethod.Parameters.Length; i++)
            {
                var paramInfo = _devMethod.Parameters[i];
                var newValue = paramInfo.HasDefaultValue 
                    ? paramInfo.DefaultValue 
                    : (paramInfo.ParameterType.IsValueType ? Activator.CreateInstance(paramInfo.ParameterType) : null);
                
                _currentParameters[i] = newValue;
            }
            SaveToEditorPrefs();
        }

        public void SaveToEditorPrefs()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
            
            var methodKey = _devMethod.MethodInfo.ToString();
            for (int i = 0; i < _devMethod.Parameters.Length; i++)
            {
                var paramName = _devMethod.Parameters[i].Name;
                var value = _currentParameters[i];
                var stringValue = DevMenuContainer.ParameterHandlerRegistry.ToString(value);
                DevMenuParameterPrefs.SetValue(methodKey, paramName, stringValue);
            }
#endif
        }

        public bool LoadFromEditorPrefs()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return false;
            
            var methodKey = _devMethod.MethodInfo.ToString();
            var allLoaded = true;
            
            for (int i = 0; i < _devMethod.Parameters.Length; i++)
            {
                var paramName = _devMethod.Parameters[i].Name;
                var savedValue = DevMenuParameterPrefs.GetValue(methodKey, paramName);
                
                if (!string.IsNullOrEmpty(savedValue))
                {
                    try
                    {
                        _currentParameters[i] = DevMenuContainer.ParameterHandlerRegistry.Parse(
                            savedValue, _devMethod.Parameters[i].ParameterType);
                    }
                    catch
                    {
                        allLoaded = false;
                    }
                }
                else
                {
                    allLoaded = false;
                }
            }
            
            return allLoaded;
#else
            return false;
#endif
        }
    }
} 