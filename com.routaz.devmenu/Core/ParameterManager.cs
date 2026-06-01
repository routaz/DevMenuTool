using System;
using System.Reflection;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Runtime.Interfaces;
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
        private readonly ParameterInfo[] _parameters;
        private readonly string _methodKey;
        private readonly IParameterHandlerRegistry _registry;
        private object[] _currentParameters;
        private bool _parametersInitialized = false;

        public ParameterManager(ParameterInfo[] parameters, string methodKey, IParameterHandlerRegistry registry)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _methodKey = methodKey;
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _currentParameters = new object[_parameters.Length];
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

            _currentParameters[index] = value;
            SaveToEditorPrefs();
        }

        public void ResetToDefaults()
        {
            for (int i = 0; i < _parameters.Length; i++)
            {
                var paramInfo = _parameters[i];
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

            for (int i = 0; i < _parameters.Length; i++)
            {
                var paramName = _parameters[i].Name;
                var value = _currentParameters[i];
                var stringValue = _registry.ToString(value);
                DevMenuParameterPrefs.SetValue(_methodKey, paramName, stringValue);
            }
#endif
        }

        public bool LoadFromEditorPrefs()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return false;

            var allLoaded = true;

            for (int i = 0; i < _parameters.Length; i++)
            {
                var paramName = _parameters[i].Name;
                var savedValue = DevMenuParameterPrefs.GetValue(_methodKey, paramName);

                if (!string.IsNullOrEmpty(savedValue))
                {
                    try
                    {
                        _currentParameters[i] = _registry.Parse(
                            savedValue, _parameters[i].ParameterType);
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
