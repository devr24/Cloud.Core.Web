namespace Cloud.Core.Web
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using System;

    /// <summary>
    /// Attribute filter that can be added to a class or method to enable
    /// model validation without the need for code.  Returns "BadRequestObjectResult" (400 response)
    /// with model state within the body.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new ValidationFailedResult(context.ModelState);
            }
        }
    }
}