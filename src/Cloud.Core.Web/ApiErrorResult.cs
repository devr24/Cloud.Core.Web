namespace Cloud.Core.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Newtonsoft.Json;

    /// <summary>
    /// Api error results model.
    /// </summary>
    public class ApiErrorResult
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; }

        /// <summary>
        /// Gets the errors.
        /// </summary>
        /// <value>
        /// The errors.
        /// </value>
        public List<ApiError> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiErrorResult" /> class with model state.
        /// </summary>
        /// <param name="modelState">State of the model.</param>
        /// <param name="defaultMessage">The default message for the error result [if set].</param>
        public ApiErrorResult(ModelStateDictionary modelState, string defaultMessage = "Api error")
        {
            Message = defaultMessage; // Default validation error message.
            Errors = modelState.Keys
                .SelectMany(key => modelState[key].Errors.Select(x => new ApiError(key, x.ErrorMessage)))
                .ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiErrorResult"/> class with exception.
        /// </summary>
        /// <param name="ex">The exception to log..</param>
        /// <param name="defaultMessage">The default message.</param>
        public ApiErrorResult(Exception ex, string defaultMessage)
        {
            Message = defaultMessage;
            Errors = new List<ApiError>{ new ApiError(ex.Message) };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiErrorResult"/> class with string.
        /// </summary>
        /// <param name="errorDescription">The custom description of what went wrong.</param>
        public ApiErrorResult(string errorDescription)
        {
            Message = errorDescription;
            Errors = new List<ApiError> { new ApiError(errorDescription) };
        }
    }

    /// <summary>
    /// Api error containing a field and message associated with the field.
    /// </summary>
    public class ApiError
    {
        /// <summary>
        /// Object can only be constructed internally - to clarify that users should create the
        /// ApiErrorResult object, not the ApiError object.
        /// </summary>
        internal ApiError() { }

        /// <summary>
        /// Gets the field name.
        /// </summary>
        /// <value>
        /// The field.
        /// </value>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Field { get; }

        /// <summary>
        /// Gets the api error message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiError"/> class with field and message.
        /// </summary>
        /// <param name="field">The field name.</param>
        /// <param name="message">The api error message.</param>
        public ApiError(string field, string message)
        {
            Field = field != string.Empty ? field : null;
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiError"/> class with message.
        /// </summary>
        /// <param name="message">The message.</param>
        public ApiError(string message)
        {
            Message = message;
        }
    }
}
