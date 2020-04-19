namespace Cloud.Core.Web
{
    using System;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    /// <summary>
    /// Validation failed result with object body.
    /// </summary>
    /// <seealso cref="ObjectResult" />
    public class ValidationFailedResult : ObjectResult
    {
        public ValidationFailedResult(ModelStateDictionary modelState)
            : base(new ApiErrorResult(modelState, "Validation Error"))
        {
            StatusCode = StatusCodes.Status400BadRequest; 
        }

        public override string ToString()
        {
            var err = Value as ApiErrorResult;
            var sb = new StringBuilder();

            if (err != null)
            { 
                foreach (var item in err.Errors)
                {
                    sb.AppendFormat("{0} {1} - {2}", 
                        sb.Length > 0 ? "," : string.Empty,
                        item.Field, 
                        item.Message);
                }
            }

            return $"Validation Error ({StatusCode}): Model could not be validated.{sb}";
        }
    }

    /// <summary>
    /// Internal server error result with object body.
    /// </summary>
    /// <seealso cref="ObjectResult" />
    public class InternalServerErrorResult : ObjectResult
    {
        public InternalServerErrorResult(Exception ex)
            : base(new ApiErrorResult(ex, "Internal Server Error"))
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}
