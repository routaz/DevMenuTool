using System;
using System.Collections.Generic;
using System.Reflection;
using DevMenuTool.Core.Interfaces;
using UnityEngine;

namespace DevMenuTool.Core
{
    /// <summary>
    /// Manages parameter validation for a DevMethod.
    /// </summary>
    public class ParameterValidator : IParameterValidatorManager
    {
        private readonly ParameterInfo[] _parameters;
        private readonly Dictionary<int, List<IParameterValidator>> _validators = new();

        public ParameterValidator(ParameterInfo[] parameters)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public void AddValidator(int parameterIndex, IParameterValidator validator)
        {
            if (parameterIndex < 0 || parameterIndex >= _parameters.Length || validator == null)
                return;

            if (!_validators.ContainsKey(parameterIndex))
                _validators[parameterIndex] = new List<IParameterValidator>();

            _validators[parameterIndex].Add(validator);
        }

        public void RemoveValidator(int parameterIndex, IParameterValidator validator)
        {
            if (parameterIndex < 0 || parameterIndex >= _parameters.Length || validator == null)
                return;

            if (_validators.ContainsKey(parameterIndex))
                _validators[parameterIndex].Remove(validator);
        }

        public void ClearValidators(int parameterIndex)
        {
            if (parameterIndex < 0 || parameterIndex >= _parameters.Length)
                return;

            _validators.Remove(parameterIndex);
        }

        public ParameterValidationResult ValidateParameter(int parameterIndex, object value)
        {
            if (parameterIndex < 0 || parameterIndex >= _parameters.Length)
                return ParameterValidationResult.Failure("Invalid parameter index");

            var parameterType = _parameters[parameterIndex].ParameterType;
            var parameterName = _parameters[parameterIndex].Name;
            
            if (_validators.ContainsKey(parameterIndex))
            {
                foreach (var validator in _validators[parameterIndex])
                {
                    var result = validator.Validate(parameterName, value, parameterType);
                    if (!result.IsValid)
                        return result;
                }
            }

            return ParameterValidationResult.Success();
        }
    }
} 