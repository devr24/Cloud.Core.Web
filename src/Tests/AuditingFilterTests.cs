using System.Collections.Generic;
using Cloud.Core.Web.Filters;
using Cloud.Core.Testing;
using Cloud.Core.Web.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc.Routing;
using Cloud.Core.Web.Tests.Fakes;
using FluentAssertions;

namespace Cloud.Core.Web.Tests
{
    [IsUnit]
    public class AuditingFilterTests
    {
        /// <summary>Ensure parameters passed to constructor are set accordingly.</summary>
        [Fact]
        public void Test_AuditingAttribute()
        {
            // Arrange
            var att = new AuditingAttribute(null, null);
            var paramsAttr = new AuditingAttribute("param1", "param2");

            // Act/Assert
            att.Arguments.Length.Should().Be(2);
            paramsAttr.Arguments.Length.Should().Be(2);
        }

        /// <summary>Ensure log message is set as exected.</summary>
        [Fact]
        public void Test_AuditingFilter_LogCorrectly()
        {
            // Arrange
            var mockAuditLogger = new Mock<IAuditLogger>();
            var filter = new AuditingFilter("EventName", "EventMessage", mockAuditLogger.Object);
            var context = new ActionExecutingContext(new ActionContext
                {
                    HttpContext = FakeHttpContext.GetRequestHttpContext(new byte[] { }),
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
                }, new Mock<ControllerBase>().Object);

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

        /// <summary>Ensure audit message is set correctly when no route name is supplied.</summary>
        [Fact]
        public void Test_AuditingFilter_NoRouteName()
        {
            // Arrange
            var mockAuditLogger = new Mock<IAuditLogger>();
            var filter = new AuditingFilter("EventName", "EventMessage", mockAuditLogger.Object);
            var context = new ActionExecutingContext(new ActionContext
                {
                    HttpContext = FakeHttpContext.GetRequestHttpContext(new byte[] { }),
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                    {
                        AttributeRouteInfo = new AttributeRouteInfo()
                        {
                        },
                        DisplayName = "TestDisplayName"
                    }
                }, new List<IFilterMetadata>(),
                new Dictionary<string, object>()
                {
                    { "id", 1 }
                }, new Mock<ControllerBase>().Object);

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

        /// <summary>Ensure audit message is set correctly when no route name and parameter is supplied.</summary>
        [Fact]
        public void Test_AuditingFilter_NoIdParameter()
        {
            // Arrange
            var mockAuditLogger = new Mock<IAuditLogger>();
            var filter = new AuditingFilter("EventName", "EventMessage", mockAuditLogger.Object);
            var context = new ActionExecutingContext(new ActionContext
                {
                    HttpContext = FakeHttpContext.GetRequestHttpContext(new byte[] { }),
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                    {
                        AttributeRouteInfo = new AttributeRouteInfo()
                        {
                        },
                        DisplayName = "TestDisplayName"
                    }
                }, new List<IFilterMetadata>(), new Dictionary<string, object>(), new Mock<ControllerBase>().Object);

            // Act
            filter.OnActionExecuting(context);

            // Assert
            mockAuditLogger.Verify(m =>m.WriteLog(
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
