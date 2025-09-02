using System;
using System.Reflection;
using UnityEngine;

namespace DevMenuTool.Core.Interfaces
{
    public interface IDevMethod : IDevMethodInfo, IDevMethodExecutor
    {
        // Parameter management methods
        object[] GetCurrentParameters();
        void SetParameter(int index, object value);
        void ResetToDefaults();
        void SaveToEditorPrefs();
        bool LoadFromEditorPrefs();

        // Parameter validation methods
        void AddValidator(int parameterIndex, IParameterValidator validator);
        void RemoveValidator(int parameterIndex, IParameterValidator validator);
        void ClearValidators(int parameterIndex);
        ParameterValidationResult ValidateParameter(int parameterIndex, object value);

        // Parameter change events
        event EventHandler<ParameterChangeEventArgs> ParameterChanged;
        void AddParameterChangeHandler(IParameterChangeHandler handler);
        void RemoveParameterChangeHandler(IParameterChangeHandler handler);
    }
}
