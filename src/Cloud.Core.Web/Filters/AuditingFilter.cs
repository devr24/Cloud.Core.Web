using System;
using System.Collections.Generic;
using System.Linq;
using Cloud.Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Cloud.Core.Web.Filters
{
    /// <summary>
    /// Filter for auditing Web API actions
    /// </summary>
    public class AuditingFilter : IActionFilter
    {
        private readonly string _eventName;
        private readonly string _eventMessage;
        private readonly IAuditLogger _auditLogger;

        /// <summary>
        /// Constructor takes in some event specific info.
        /// </summary>
        public AuditingFilter(string eventName, string eventMessage, IAuditLogger auditLogger)
        {
            _eventName = eventName;
            _eventMessage = eventMessage;
            _auditLogger = auditLogger;
        }

        /// <summary>
        /// Unused.
        /// </summary>
        /// <param name="context">Action execution context.</param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //Do nothing
        }

        /// <summary>
        /// Auditing to permanent storage is done using some common controller action info.
        /// </summary>
        /// <param name="context">Action execution context.</param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            //Audit info
            var eventType = context.ActionDescriptor.AttributeRouteInfo.Name.SetDefaultIfNullOrEmpty("No Route Name Supplied");
            var eventTargetId = context.ActionArguments.FirstOrDefault(x => x.Key.Equals("id", StringComparison.OrdinalIgnoreCase)).Value ?? "No id Parameter Supplied";
            var eventSource = context.ActionDescriptor.DisplayName; // This is the full assembly name including controller and action

            string userId = context.HttpContext.GetUserId();
            string referer = context.HttpContext.GetRequestReferer();
            
            // default if the user is not set.
            if (userId.IsNullOrDefault())
            {
                userId = AppDomain.CurrentDomain.FriendlyName.ToUpperInvariant();
            }

            var auditInfo = new Dictionary<string, string>
            {
                { "EventType", eventType },
                { "EventTargetId", eventTargetId.ToString() }
            };

            // Append referer url to the audit info if we have any.
            if (!referer.IsNullOrEmpty())
            {
                auditInfo.Add("Referer", referer);
            }

            // Create the Audit log.
            _auditLogger.WriteLog(_eventName, _eventMessage, userId, eventSource, auditInfo);
        }
    }
}
