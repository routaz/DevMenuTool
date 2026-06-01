using System;
using System.Text.RegularExpressions;
using DevMenuTool.Core.Interfaces;

namespace DevMenuTool.Core.Validators
{
    /// <summary>
    /// Validates string parameters with various constraints.
    /// </summary>
    public class StringValidator : IParameterValidator
    {
        private readonly int? _minLength;
        private readonly int? _maxLength;
        private readonly string _pattern;
        private readonly bool _allowEmpty;

        public StringValidator(int? minLength = null, int? maxLength = null, string pattern = null, bool allowEmpty = true)
        {
            _minLength = minLength;
            _maxLength = maxLength;
            _pattern = pattern;
            _allowEmpty = allowEmpty;
        }

        public ParameterValidationResult Validate(string parameterName, object value, Type parameterType)
        {
            if (parameterType != typeof(string))
                return ParameterValidationResult.Success();

            var stringValue = value as string;
            
            if (string.IsNullOrEmpty(stringValue))
            {
                if (!_allowEmpty)
                {
                    return ParameterValidationResult.Failure($"Parameter '{parameterName}' cannot be null or empty.");
                }
                return ParameterValidationResult.Success();
            }
            
            if (_minLength.HasValue && stringValue.Length < _minLength.Value)
            {
                return ParameterValidationResult.Failure(
                    $"Parameter '{parameterName}' must be at least {_minLength.Value} characters long. Current length: {stringValue.Length}");
            }
            
            if (_maxLength.HasValue && stringValue.Length > _maxLength.Value)
            {
                return ParameterValidationResult.Failure(
                    $"Parameter '{parameterName}' must be no more than {_maxLength.Value} characters long. Current length: {stringValue.Length}");
            }
            
            if (!string.IsNullOrEmpty(_pattern))
            {
                try
                {
                    var regex = new Regex(_pattern);
                    if (!regex.IsMatch(stringValue))
                    {
                        return ParameterValidationResult.Failure(
                            $"Parameter '{parameterName}' does not match the required pattern: {_pattern}");
                    }
                }
                catch (ArgumentException ex)
                {
                    return ParameterValidationResult.Failure($"Invalid regex pattern for parameter '{parameterName}': {ex.Message}");
                }
            }

            return ParameterValidationResult.Success();
        }
    }
} 