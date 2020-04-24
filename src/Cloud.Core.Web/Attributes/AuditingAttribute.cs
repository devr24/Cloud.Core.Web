using Cloud.Core.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Cloud.Core.Web.Attributes
{
    /// <summary>
    /// This attribute is handled by the Auditing Filter.
    /// The auditing filter logs common info, event specific info is passed in using this attribute
    /// </summary>
    /// 
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuditingAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Event Specific Info
        /// </summary>
        /// <param name="eventType">Resource being invoked e.g. Access Policies</param>
        /// <param name="eventMessage">Custom message that goes with the auditing</param>
        public AuditingAttribute(string eventType, string eventMessage) : base(typeof(AuditingFilter))
        {
            Arguments = new object[] { eventType, eventMessage };
        }
    }
}
