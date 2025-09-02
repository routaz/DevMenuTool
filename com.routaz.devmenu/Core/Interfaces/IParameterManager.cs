using System;

namespace DevMenuTool.Core.Interfaces
{
    /// <summary>
    /// Manages parameter storage, persistence, and state for a DevMethod.
    /// </summary>
    public interface IParameterManager
    {
        object[] GetCurrentParameters();
        void SetParameter(int index, object value);
        void ResetToDefaults();
        void SaveToEditorPrefs();
        bool LoadFromEditorPrefs();
    }
} 