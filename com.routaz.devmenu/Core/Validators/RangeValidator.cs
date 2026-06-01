using System;
using DevMenuTool.Core.Interfaces;

namespace DevMenuTool.Core.Validators
{
    /// <summary>
    /// Validates numeric parameters within a specified range.
    /// </summary>
    public class RangeValidator : IParameterValidator
    {
        private readonly double _minValue;
        private readonly double _maxValue;
        private readonly bool _inclusive;

        public RangeValidator(double minValue, double maxValue, bool inclusive = true)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _inclusive = inclusive;
        }

        public ParameterValidationResult Validate(string parameterName, object value, Type parameterType)
        {
            if (value == null)
                return ParameterValidationResult.Success();
            
            if (!IsNumericType(parameterType))
                return ParameterValidationResult.Success();

            var numericValue = Convert.ToDouble(value);

            if (_inclusive)
            {
                if (numericValue < _minValue || numericValue > _maxValue)
                {
                    return ParameterValidationResult.Failure(
                        $"Parameter '{parameterName}' must be between {_minValue} and {_maxValue} (inclusive). Current value: {numericValue}");
                }
            }
            else
            {
                if (numericValue <= _minValue || numericValue >= _maxValue)
                {
                    return ParameterValidationResult.Failure(
                        $"Parameter '{parameterName}' must be between {_minValue} and {_maxValue} (exclusive). Current value: {numericValue}");
                }
            }

            return ParameterValidationResult.Success();
        }

        private bool IsNumericType(Type type)
        {
            return type == typeof(int) || type == typeof(float) || type == typeof(double) || 
                   type == typeof(long) || type == typeof(short) || type == typeof(byte) ||
                   type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) ||
                   type == typeof(sbyte);
        }
    }
} 