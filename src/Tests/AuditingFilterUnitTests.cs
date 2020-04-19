using System.Collections.Generic;
using Cloud.Core.Web.Filters;
using Cloud.Core.Testing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc.Routing;
using System;

namespace Cloud.Core.Web.Tests
{
    [IsUnit]
    public class AuditingFilterUnitTests
    {

        [Fact]
        public void AuditingFilter_LogCorrectly()
        {
            // Arrange
            var mockAuditLogger = new Mock<IAuditLogger>();
            var filter = new AuditingFilter("EventName", "EventMessage", mockAuditLogger.Object);

            var context = new ActionExecutingContext(
            new ActionContext
            {
                HttpContext = HttpContextMock.GetRequestHttpContext(new byte[] { }),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                    {
                        Name = "TestName"
                    },
                    DisplayName = "TestDisplayName"
                }
            },
            new List<IFilterMetadata>(),
            new Dictionary<string, object>()
            {
                { "id", 1 }
            },
            new Mock<ControllerBase>().Object
            );

            // Act
            filter.OnActionExecuting(context);

            // Assert
            mockAuditLogger.Verify(m => 
            m.WriteLog(
                It.Is<string>(x => x.Equals("EventName")),
                It.Is<string>(x => x.Equals("EventMessage")),
                It.Is<string>(x => x.Equals("TestUser")),
                It.Is<string>(x => x.Equals("TestDisplayName")),
                It.Is<Dictionary<string, string>>(x =>
                    x["EventType"].Equals("TestName") &&
                    x["EventTargetId"].Equals("1")
                )
            ));
        }

        [Fact]
        public void AuditingFilter_NoRouteName()
        {
            // Arrange
            var mockAuditLogger = new Mock<IAuditLogger>();
            var filter = new AuditingFilter("EventName", "EventMessage", mockAuditLogger.Object);

            var context = new ActionExecutingContext(
            new ActionContext
            {
                HttpContext = HttpContextMock.GetRequestHttpContext(new byte[] { }),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                    {
                    },
                    DisplayName = "TestDisplayName"
                }
            },
            new List<IFilterMetadata>(),
            new Dictionary<string, object>()
            {
                { "id", 1 }
            },
            new Mock<ControllerBase>().Object
            );

            // Act
            filter.OnActionExecuting(context);
            filter.OnActionExecuted(null);

            // Assert
            mockAuditLogger.Verify(m =>
            m.WriteLog(
                It.Is<string>(x => x.Equals("EventName")),
                It.Is<string>(x => x.Equals("EventMessage")),
                It.Is<string>(x => x.Equals("TestUser")),
                It.Is<string>(x => x.Equals("TestDisplayName")),
                It.Is<Dictionary<string, string>>(x =>
                    x["EventType"].Equals("No Route Name Supplied") &&
                    x["EventTargetId"].Equals("1")
                )
            ));
        }

        [Fact]
        public void AuditingFilter_NoIdParameter()
        {
            // Arrange
            var mockAuditLogger = new Mock<IAuditLogger>();
            var filter = new AuditingFilter("EventName", "EventMessage", mockAuditLogger.Object);

            var context = new ActionExecutingContext(
            new ActionContext
            {
                HttpContext = HttpContextMock.GetRequestHttpContext(new byte[] { }),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                    {
                    },
                    DisplayName = "TestDisplayName"
                }
            },
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            new Mock<ControllerBase>().Object
            );

            // Act
            filter.OnActionExecuting(context);

            // Assert
            mockAuditLogger.Verify(m =>
            m.WriteLog(
                It.Is<string>(x => x.Equals("EventName")),
                It.Is<string>(x => x.Equals("EventMessage")),
                It.Is<string>(x => x.Equals("TestUser")),
                It.Is<string>(x => x.Equals("TestDisplayName")),
                It.Is<Dictionary<string, string>>(x =>
                    x["EventType"].Equals("No Route Name Supplied") &&
                    x["EventTargetId"].Equals("No id Parameter Supplied")
                )
            ));
        }
    }
}
