using System;

namespace DevMenuTool.Core.Interfaces
{
    /// <summary>
    /// Exposes parameter change notifications to consumers.
    /// </summary>
    public interface IParameterChangeNotifier
    {
        event EventHandler<ParameterChangeEventArgs> ParameterChanged;
        void AddParameterChangeHandler(IParameterChangeHandler handler);
        void RemoveParameterChangeHandler(IParameterChangeHandler handler);
    }
}
