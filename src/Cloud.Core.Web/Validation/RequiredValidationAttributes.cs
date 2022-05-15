#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace Cloud.Core.Web.Validation
{
    /// <summary>
    /// Class RequiredAttribute.
    /// Implements the <see cref="ValidationAttribute" />
    /// </summary>
    /// <seealso cref="ValidationAttribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class RequiredAttribute : ValidationAttribute
    {
        /// <summary>
        /// Constructor overload.
        /// </summary>
        /// <param name="errorCode">String error code.</param>
        /// <param name="errorMessage">String error message.</param>
        public RequiredAttribute(string errorCode, string errorMessage)
            : base(() => $"{errorCode}|{errorMessage}")
        {
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the attribute should allow empty strings.
        /// </summary>
        public bool AllowEmptyStrings { get; set; }

        /// <summary>
        /// Override of <see cref="ValidationAttribute.IsValid(object?)" />
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns>
        /// <c>false</c> if the <paramref name="value" /> is null or an empty string. If
        /// <see cref="AllowEmptyStrings" />
        /// then <c>false</c> is returned only if <paramref name="value" /> is null.
        /// </returns>
        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return false;
            }

            // only check string length if empty strings are not allowed
            return AllowEmptyStrings || !(value is string stringValue) || stringValue.Trim().Length != 0;
        }
    }

    /// <summary>
    /// Class EmailAddressAttribute.
    /// Implements the <see cref="DataTypeAttribute" />
    /// </summary>
    /// <seealso cref="DataTypeAttribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class EmailAddressAttribute : DataTypeAttribute
    {
        /// <summary>
        /// Constructor overload.
        /// </summary>
        /// <param name="errorCode">String error code.</param>
        /// <param name="errorMessage">String error message.</param>
        public EmailAddressAttribute(string errorCode, string errorMessage)
            : base(DataType.EmailAddress)
        {
            this.ErrorMessage = $"{errorCode}|{errorMessage}";
        }

        /// <summary>
        /// Checks that the value of the data field is valid.
        /// </summary>
        /// <param name="value">The data field value to validate.</param>
        /// <returns><see langword="true" /> always.</returns>
        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return true;
            }

            if (!(value is string valueAsString))
            {
                return false;
            }

            // only return true if there is only 1 '@' character
            // and it is neither the first nor the last character
            int index = valueAsString.IndexOf('@');

            return
                index > 0 &&
                index != valueAsString.Length - 1 &&
                index == valueAsString.LastIndexOf('@');
        }
    }
}
