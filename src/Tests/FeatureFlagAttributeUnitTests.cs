using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cloud.Core.Testing;
using Cloud.Core.Web.Attributes;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace Cloud.Core.Web.Tests
{
    [IsUnit]
    public class FeatureFlagAttributeUnitTests
    {
        [Fact]
        public void RoleRequirementAttribute()
        {
            var att = new RoleRequirementAttribute();
            ((string[])att.Arguments[0]).Length.Should().Be(0);

            var paramsAttr = new RoleRequirementAttribute(new[] { "param1", "param2" });
            ((string[])paramsAttr.Arguments[0]).Length.Should().Be(2);
        }

        [Fact]
        public void AuditingAttribute()
        {
            var att = new AuditingAttribute(null, null);
            att.Arguments.Length.Should().Be(2);

            var paramsAttr = new AuditingAttribute("param1", "param2" );
            paramsAttr.Arguments.Length.Should().Be(2);
        }

        [Fact]
        public void FeatureFlagAttribute_FlagOn()
        {
            // Arrange
            var mockFeatureFlags = new Mock<IFeatureFlag>();
            mockFeatureFlags.Setup(
                m => m.GetFeatureFlag(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                )).Returns(true);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(m => m.GetService(It.IsAny<Type>())).Returns(mockFeatureFlags.Object);

            var featureFlag = new FeatureFlagAttribute("featureFlagKey");

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
                    }
                }
            },
            new List<IFilterMetadata>(),
            new Dictionary<string, object>()
            {
                { "id", 1 }
            },
            new Mock<ControllerBase>().Object
            );
            context.HttpContext.RequestServices = mockServiceProvider.Object;

            // Act
            featureFlag.OnActionExecuting(context);

            // Assert
            Assert.Null(context.Result);

            mockFeatureFlags.Verify(m => 
            m.GetFeatureFlag(
                It.IsAny<string>(),
                It.IsAny<bool>()
                ),
                Times.Once);
        }

        [Fact]
        public void FeatureFlagAttribute_FlagOff()
        {
            // Arrange
            var mockFeatureFlags = new Mock<IFeatureFlag>();
            mockFeatureFlags.Setup(
                m => m.GetFeatureFlag(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                )).Returns(false);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(m => m.GetService(It.IsAny<Type>())).Returns(mockFeatureFlags.Object);

            var featureFlag = new FeatureFlagAttribute("featureFlagKey");

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
                    }
                }
            },
            new List<IFilterMetadata>(),
            new Dictionary<string, object>()
            {
                { "id", 1 }
            },
            new Mock<ControllerBase>().Object
            );
            context.HttpContext.RequestServices = mockServiceProvider.Object;

            // Act
            featureFlag.OnActionExecuting(context);
            var result = context.Result as NotFoundObjectResult;
            var apiError = result.Value as ApiErrorResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("This route has been disabled.", apiError.Errors.First().Message);

            mockFeatureFlags.Verify(m =>
            m.GetFeatureFlag(
                It.IsAny<string>(),
                It.IsAny<bool>()
                ),
                Times.Once);
        }

        [Fact]
        public void FeatureFlagAttribute_NoFeatureFlagServiceRegistered()
        {
            // Arrange
            var featureFlag = new FeatureFlagAttribute("featureFlagKey");
            var mockServiceProvider = new Mock<IServiceProvider>();

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
                    }
                }
            },
            new List<IFilterMetadata>(),
            new Dictionary<string, object>()
            {
                { "id", 1 }
            },
            new Mock<ControllerBase>().Object
            );
            context.HttpContext.RequestServices = mockServiceProvider.Object;

            // Act
            // Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                featureFlag.OnActionExecuting(context);
            });

            Assert.Equal("No feature flag service registered", ex.Message);
        }
    }
}
