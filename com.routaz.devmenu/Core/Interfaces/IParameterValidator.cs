using System;

namespace DevMenuTool.Core.Interfaces
{
    /// <summary>
    /// Interface for validating parameter values before they are set.
    /// </summary>
    public interface IParameterValidator
    {
        /// <summary>
        /// Validates a parameter value.
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="value">Value to validate</param>
        /// <param name="parameterType">Type of the parameter</param>
        /// <returns>Validation result with success status and error message</returns>
        ParameterValidationResult Validate(string parameterName, object value, Type parameterType);
    }

    /// <summary>
    /// Result of parameter validation.
    /// </summary>
    public struct ParameterValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        public ParameterValidationResult(bool isValid, string errorMessage = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public static ParameterValidationResult Success() => new ParameterValidationResult(true);
        public static ParameterValidationResult Failure(string errorMessage) => new ParameterValidationResult(false, errorMessage);
    }
} 