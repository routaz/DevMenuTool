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
        private readonly ParameterInfo[] _parameters;
        private readonly string _methodName;
        private readonly List<IParameterChangeHandler> _changeHandlers = new();

        public ParameterEventManager(ParameterInfo[] parameters, string methodName)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _methodName = methodName;
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

        public void OnParameterChanged(object sender, int parameterIndex, object oldValue, object newValue)
        {
            var parameterName = _parameters[parameterIndex].Name;
            var parameterType = _parameters[parameterIndex].ParameterType;
            var eventArgs = new ParameterChangeEventArgs(_methodName, parameterName, oldValue, newValue, parameterType);
            
            ParameterChanged?.Invoke(sender, eventArgs);
            
            foreach (var handler in _changeHandlers)
            {
                try
                {
                    handler.OnParameterChanged(sender, eventArgs);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in parameter change handler: {ex.Message}");
                }
            }
        }
    }
} 