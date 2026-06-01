namespace DevMenuTool.Core.Interfaces
{
    /// <summary>
    /// Manages the collection of validators attached to a DevMethod's parameters
    /// and validates values against them.
    /// </summary>
    public interface IParameterValidatorManager
    {
        void AddValidator(int parameterIndex, IParameterValidator validator);
        void RemoveValidator(int parameterIndex, IParameterValidator validator);
        void ClearValidators(int parameterIndex);
        ParameterValidationResult ValidateParameter(int parameterIndex, object value);
    }
}
