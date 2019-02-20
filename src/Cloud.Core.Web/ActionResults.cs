namespace Cloud.Core.Web
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    /// <summary>
    /// Validation failed result with object body.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ObjectResult" />
    public class ValidationFailedResult : ObjectResult
    {
        public ValidationFailedResult(ModelStateDictionary modelState)
            : base(new ApiErrorResult(modelState, "Validation Error"))
        {
            StatusCode = StatusCodes.Status400BadRequest; 
            // Note, possibly use 422 instead: StatusCodes.Status422 - Un-processable Entity;
        }
    }

    /// <summary>
    /// Internal server error result with object body.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ObjectResult" />
    public class InternalServerErrorResult : ObjectResult
    {
        public InternalServerErrorResult(Exception ex)
            : base(new ApiErrorResult(ex, "Internal Server Error"))
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}
