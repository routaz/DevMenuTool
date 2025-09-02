using System;
using System.Collections.Generic;
using System.Reflection;
using DevMenuTool.Core.Interfaces;
using UnityEngine;

namespace DevMenuTool.Core
{
    /// <summary>
    /// Manages parameter change events and handlers for a DevMethod.
    /// </summary>
    public class ParameterEventManager : IParameterEventManager
    {
        private readonly IDevMethod _devMethod;
        private readonly List<IParameterChangeHandler> _changeHandlers = new();

        public ParameterEventManager(IDevMethod devMethod)
        {
            _devMethod = devMethod ?? throw new ArgumentNullException(nameof(devMethod));
        }

        public event EventHandler<ParameterChangeEventArgs> ParameterChanged;

        public void AddParameterChangeHandler(IParameterChangeHandler handler)
        {
            if (handler != null && !_changeHandlers.Contains(handler))
                _changeHandlers.Add(handler);
        }

        public void RemoveParameterChangeHandler(IParameterChangeHandler handler)
        {
            if (handler != null)
                _changeHandlers.Remove(handler);
        }

        public void OnParameterChanged(int parameterIndex, object oldValue, object newValue)
        {
            var parameterName = _devMethod.Parameters[parameterIndex].Name;
            var parameterType = _devMethod.Parameters[parameterIndex].ParameterType;
            var eventArgs = new ParameterChangeEventArgs(_devMethod.MethodInfo.Name, parameterName, oldValue, newValue, parameterType);

            // Trigger the event
            ParameterChanged?.Invoke(_devMethod, eventArgs);

            // Notify all change handlers
            foreach (var handler in _changeHandlers)
            {
                try
                {
                    handler.OnParameterChanged(_devMethod, eventArgs);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in parameter change handler: {ex.Message}");
                }
            }
        }
    }
} 