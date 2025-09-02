using System;

namespace DevMenuTool.Core.Interfaces
{
    /// <summary>
    /// Manages parameter change events and handlers for a DevMethod.
    /// </summary>
    public interface IParameterEventManager
    {
        event EventHandler<ParameterChangeEventArgs> ParameterChanged;
        void AddParameterChangeHandler(IParameterChangeHandler handler);
        void RemoveParameterChangeHandler(IParameterChangeHandler handler);
        void OnParameterChanged(int parameterIndex, object oldValue, object newValue);
    }
} 