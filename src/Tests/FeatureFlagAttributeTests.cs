using System;
using System.Collections.Generic;
using System.Linq;
using Cloud.Core.Testing;
using Cloud.Core.Web.Attributes;
using Cloud.Core.Web.Tests.Fakes;
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
    public class FeatureFlagAttributeTests
    {
        /// <summary>Ensure feature flag is switched on as expected.</summary>
        [Fact]
        public void FeatureFlagAttribute_FlagOn()
        {
            // Arrange
            var mockFeatureFlags = new Mock<IFeatureFlag>();
            mockFeatureFlags.Setup(m => m.GetFeatureFlag(It.IsAny<string>(), It.IsAny<bool>())).Returns(true);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(m => m.GetService(It.IsAny<Type>())).Returns(mockFeatureFlags.Object);

            var featureFlag = new FeatureFlagAttribute("featureFlagKey");
            var context = new ActionExecutingContext(new ActionContext
                {
                    HttpContext = FakeHttpContext.GetRequestHttpContext(new byte[] { }),
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
                new Mock<ControllerBase>().Object);
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

        /// <summary>Ensure feature flag is switched off as expected.</summary>
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
            var context = new ActionExecutingContext(new ActionContext
                {
                    HttpContext = FakeHttpContext.GetRequestHttpContext(new byte[] { }),
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
                }, new Mock<ControllerBase>().Object);
            context.HttpContext.RequestServices = mockServiceProvider.Object;

            // Act
            featureFlag.OnActionExecuting(context);
            var result = context.Result as NotFoundObjectResult;
            var apiError = result.Value as Validation.ValidationProblemDetails;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("This route has been disabled.", apiError.ErrorItems.First().Key);
            
            mockFeatureFlags.Verify(m =>
            m.GetFeatureFlag(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
        }

        /// <summary>Ensure no feature flag service is registered and expected error message is set.</summary>
        [Fact]
        public void FeatureFlagAttribute_NoFeatureFlagServiceRegistered()
        {
            // Arrange
            var featureFlag = new FeatureFlagAttribute("featureFlagKey");
            var mockServiceProvider = new Mock<IServiceProvider>();

            var context = new ActionExecutingContext(new ActionContext
                {
                    HttpContext = FakeHttpContext.GetRequestHttpContext(new byte[] { }),
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
                }, new Mock<ControllerBase>().Object);
            context.HttpContext.RequestServices = mockServiceProvider.Object;

            // Act/Assert
            var ex = Assert.Throws<InvalidOperationException>(() => featureFlag.OnActionExecuting(context));
            Assert.Equal("No feature flag service registered", ex.Message);
        }
    }
}
