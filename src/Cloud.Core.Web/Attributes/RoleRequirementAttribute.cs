using System;
using Cloud.Core.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Cloud.Core.Web.Attributes
{
    /// <summary>
    /// Use this attribute to enforce role requirements for actions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class RoleRequirementAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Pass in roles, users must have at least one.
        /// </summary>
        /// <param name="roles">List of roles that are required.</param>
        public RoleRequirementAttribute(params string[] roles) : base(typeof(RoleRequirementFilter))
        {
            Arguments = new object[] { roles };
        }
    }
}
