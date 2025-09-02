using System;

namespace DevMenuTool.Core.Interfaces
{
    /// <summary>
    /// Event arguments for parameter change events.
    /// </summary>
    public class ParameterChangeEventArgs : EventArgs
    {
        public string MethodName { get; }
        public string ParameterName { get; }
        public object OldValue { get; }
        public object NewValue { get; }
        public Type ParameterType { get; }

        public ParameterChangeEventArgs(string methodName, string parameterName, object oldValue, object newValue, Type parameterType)
        {
            MethodName = methodName;
            ParameterName = parameterName;
            OldValue = oldValue;
            NewValue = newValue;
            ParameterType = parameterType;
        }
    }

    /// <summary>
    /// Interface for components that can handle parameter change events.
    /// </summary>
    public interface IParameterChangeHandler
    {
        /// <summary>
        /// Called when a parameter value changes.
        /// </summary>
        /// <param name="sender">The DevMethod that triggered the event</param>
        /// <param name="e">Event arguments containing change details</param>
        void OnParameterChanged(object sender, ParameterChangeEventArgs e);
    }
} 