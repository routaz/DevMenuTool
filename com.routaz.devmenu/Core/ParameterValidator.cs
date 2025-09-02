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
    public class ParameterValidator : IParameterValidator
    {
        private readonly IDevMethod _devMethod;
        private readonly Dictionary<int, List<IParameterValidator>> _validators = new();

        public ParameterValidator(IDevMethod devMethod)
        {
            _devMethod = devMethod ?? throw new ArgumentNullException(nameof(devMethod));
        }

        public void AddValidator(int parameterIndex, IParameterValidator validator)
        {
            if (parameterIndex < 0 || parameterIndex >= _devMethod.Parameters.Length || validator == null)
                return;

            if (!_validators.ContainsKey(parameterIndex))
                _validators[parameterIndex] = new List<IParameterValidator>();

            _validators[parameterIndex].Add(validator);
        }

        public void RemoveValidator(int parameterIndex, IParameterValidator validator)
        {
            if (parameterIndex < 0 || parameterIndex >= _devMethod.Parameters.Length || validator == null)
                return;

            if (_validators.ContainsKey(parameterIndex))
                _validators[parameterIndex].Remove(validator);
        }

        public void ClearValidators(int parameterIndex)
        {
            if (parameterIndex < 0 || parameterIndex >= _devMethod.Parameters.Length)
                return;

            _validators.Remove(parameterIndex);
        }

        public ParameterValidationResult ValidateParameter(int parameterIndex, object value)
        {
            if (parameterIndex < 0 || parameterIndex >= _devMethod.Parameters.Length)
                return ParameterValidationResult.Failure("Invalid parameter index");

            var parameterType = _devMethod.Parameters[parameterIndex].ParameterType;
            var parameterName = _devMethod.Parameters[parameterIndex].Name;

            // Check if we have validators for this parameter
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

        public ParameterValidationResult Validate(string parameterName, object value, Type parameterType)
        {
            // This is the interface implementation for IParameterValidator
            // For the ParameterValidator class, we delegate to the specific parameter validation
            return ParameterValidationResult.Success();
        }
    }
} 