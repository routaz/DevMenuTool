using System;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Core.Validators;

namespace DevMenuTool.Core.Attributes
{
    /// <summary>
    /// Base attribute for parameter validation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public abstract class ParameterValidationAttribute : Attribute
    {
        public abstract IParameterValidator CreateValidator();
    }

    /// <summary>
    /// Validates that a numeric parameter is within a specified range.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class ParameterRangeAttribute : ParameterValidationAttribute
    {
        public double MinValue { get; }
        public double MaxValue { get; }
        public bool Inclusive { get; }

        public ParameterRangeAttribute(double minValue, double maxValue, bool inclusive = true)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            Inclusive = inclusive;
        }

        public override IParameterValidator CreateValidator()
        {
            return new RangeValidator(MinValue, MaxValue, Inclusive);
        }
    }

    /// <summary>
    /// Validates that a string parameter meets length and pattern requirements.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class StringValidationAttribute : ParameterValidationAttribute
    {
        public int MinLength { get; }
        public int MaxLength { get; }
        public string Pattern { get; }
        public bool AllowEmpty { get; }

        public StringValidationAttribute(int minLength = -1, int maxLength = -1, string pattern = null, bool allowEmpty = true)
        {
            MinLength = minLength;
            MaxLength = maxLength;
            Pattern = pattern;
            AllowEmpty = allowEmpty;
        }

        public override IParameterValidator CreateValidator()
        {
            var minLengthNullable = MinLength >= 0 ? (int?)MinLength : null;
            var maxLengthNullable = MaxLength >= 0 ? (int?)MaxLength : null;
            return new StringValidator(minLengthNullable, maxLengthNullable, Pattern, AllowEmpty);
        }
    }

    /// <summary>
    /// Validates that a parameter is not null or empty.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class RequiredAttribute : ParameterValidationAttribute
    {
        public override IParameterValidator CreateValidator()
        {
            return new StringValidator(allowEmpty: false);
        }
    }
} 