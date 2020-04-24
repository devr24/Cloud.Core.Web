using System.Collections.Generic;
using System.Security.Claims;
using Cloud.Core.Testing;
using Cloud.Core.Web.Attributes;
using Cloud.Core.Web.Filters;
using Cloud.Core.Web.Tests.Fakes;
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
    public class RoleRequirementFilterTests
    {
        /// <summary>Ensure params passed to attribute are set accordingly.</summary>
        [Fact]
        public void Test_RoleRequirementAttribute_Params()
        {
            // Arrange
            var att = new RoleRequirementAttribute();
            var paramsAttr = new RoleRequirementAttribute(new[] { "param1", "param2" });

            // Act/Assert
            ((string[])att.Arguments[0]).Length.Should().Be(0);
            ((string[])paramsAttr.Arguments[0]).Length.Should().Be(2);
        }

        /// <summary>Verify a feature flag check happens when a user is authorized.</summary>
        [Fact]
        public void Test_RoleRequirementFilter_NoFeatureFlagsSupplied()
        {
            // Arrange
            var mockFeatureFlags = new Mock<IFeatureFlag>();
            var filter = new RoleRequirementFilter(new string[] { "" });

            var authContext = new AuthorizationFilterContext(new ActionContext
                {
                    HttpContext = FakeHttpContext.GetRequestHttpContext(new byte[] { }),
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor() {
                        AttributeRouteInfo = new AttributeRouteInfo()
                    }
                },
                new List<IFilterMetadata>());
            authContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>
                {
                    new Claim("oid", "token")
                })
            );

            // Act
            filter.OnAuthorization(authContext);

            // Assert
            mockFeatureFlags.Verify(m => m.GetFeatureFlag(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        /// <summary>Verify is authorized with the correct role.</summary>
        [Fact]
        public void Test_RoleRequirementFilter_NoFeatureFlagsSupplied_ValidRoles()
        {
            // Arrange
            var filter = new RoleRequirementFilter(new string[] { "TestRole" });

            var authContext = new AuthorizationFilterContext(new ActionContext
                {
                    HttpContext = FakeHttpContext.GetRequestHttpContext(new byte[] { }),
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                    {
                        AttributeRouteInfo = new AttributeRouteInfo()
                    }
                }, new List<IFilterMetadata>());
            authContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>
                {
                    new Claim("oid", "token"),
                    new Claim(ClaimTypes.Role, "TestRole")
                })
            );

            // Act
            filter.OnAuthorization(authContext);

            // Assert
            Assert.Null(authContext.Result);
        }

        /// <summary>Verify not authorized with an incorrect role.</summary>
        [Fact]
        public void Test_RoleRequirementFilter_NoFeatureFlagsSupplied_InvalidRoles()
        {
            // Arrange
            var mockFeatureFlags = new Mock<IFeatureFlag>();
            var filter = new RoleRequirementFilter(new string[] { "TestRole" });

            var authContext = new AuthorizationFilterContext(new ActionContext
                {
                    HttpContext = FakeHttpContext.GetRequestHttpContext(new byte[] { }),
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                    {
                        AttributeRouteInfo = new AttributeRouteInfo()
                    }
                }, new List<IFilterMetadata>());
            authContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>
                {
                    new Claim("oid", "token"),
                    new Claim(ClaimTypes.Role, "InvalidTestRole")
                }));

            // Act
            filter.OnAuthorization(authContext);

            // Assert
            var result = authContext.Result as ForbidResult;
            Assert.NotNull(result);
        }

        /// <summary>Verify authorized when authorization is off.</summary>
        [Fact]
        public void Test_RoleRequirementFilter_FeatureFlagsSupplied_RolesAuthOff()
        {
            // Arrange
            var mockFeatureFlags = new Mock<IFeatureFlag>();
            mockFeatureFlags.Setup(m => m.GetFeatureFlag(
                It.IsAny<string>(),
                It.IsAny<bool>()
                )).Returns(false);
            var filter = new RoleRequirementFilter(new string[] { "TestRole" }, mockFeatureFlags.Object);

            var authContext = new AuthorizationFilterContext(new ActionContext
                {
                    HttpContext = FakeHttpContext.GetRequestHttpContext(new byte[] { }),
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                    {
                        AttributeRouteInfo = new AttributeRouteInfo()
                    }
                },
                new List<IFilterMetadata>());
            authContext.HttpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>
                {
                    new Claim("oid", "token"),
                    new Claim(ClaimTypes.Role, "InvalidTestRole")
                }));

            // Act
            filter.OnAuthorization(authContext);

            // Assert
            Assert.Null(authContext.Result);
        }

        /// <summary>Verify roles auth allows a call.</summary>
        [Fact]
        public void Test_RoleRequirementFilter_FeatureFlagsSupplied_RolesAuthOn_ValidRoles()
        {
            // Arrange
            var mockFeatureFlags = new Mock<IFeatureFlag>();
            mockFeatureFlags.Setup(m => m.GetFeatureFlag(
                It.IsAny<string>(),
                It.IsAny<bool>()
                )).Returns(true);
            var filter = new RoleRequirementFilter(new string[] { "TestRole" }, mockFeatureFlags.Object);

            var authContext = new AuthorizationFilterContext(
            new ActionContext
            {
                HttpContext = FakeHttpContext.GetRequestHttpContext(new byte[] { }),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                }
            },
            new List<IFilterMetadata>()
            );
            authContext.HttpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>
                {
                    new Claim("oid", "token"),
                    new Claim(ClaimTypes.Role, "TestRole")
                })
            );

            // Act
            filter.OnAuthorization(authContext);

            // Assert
            Assert.Null(authContext.Result);
        }

        /// <summary>Attempt to resolve a feature flag but unauthorized role, returns as expected.</summary>
        [Fact]
        public void Test_RoleRequirementFilter_FeatureFlagsSupplied_RolesAuthOn_InvalidRoles()
        {
            // Arrange
            var mockFeatureFlags = new Mock<IFeatureFlag>();
            mockFeatureFlags.Setup(m => m.GetFeatureFlag(
                It.IsAny<string>(),
                It.IsAny<bool>()
                )).Returns(true);
            var filter = new RoleRequirementFilter(new string[] { "TestRole" }, mockFeatureFlags.Object);

            var authContext = new AuthorizationFilterContext(
            new ActionContext
            {
                HttpContext = FakeHttpContext.GetRequestHttpContext(new byte[] { }),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                }
            },
            new List<IFilterMetadata>()
            );
            authContext.HttpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>
                {
                    new Claim("oid", "token"),
                    new Claim(ClaimTypes.Role, "InvalidTestRole")
                })
            );

            // Act
            filter.OnAuthorization(authContext);

            // Assert
            var result = authContext.Result as ForbidResult;

            Assert.NotNull(result);
        }

        /// <summary>Attempt to resolve a feature flag but unauthorized user, returns as expected.</summary>
        [Fact]
        public void Test_RoleRequirementFilter_FeatureFlagsSupplied_RolesAuthOn_NoUser()
        {
            // Arrange
            var mockFeatureFlags = new Mock<IFeatureFlag>();
            mockFeatureFlags.Setup(m => m.GetFeatureFlag(
                It.IsAny<string>(),
                It.IsAny<bool>()
                )).Returns(true);
            var filter = new RoleRequirementFilter(new string[] { "TestRole" }, mockFeatureFlags.Object);

            var authContext = new AuthorizationFilterContext(
            new ActionContext
            {
                HttpContext = FakeHttpContext.GetRequestHttpContext(new byte[] { }),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                }
            },
            new List<IFilterMetadata>()
            );
            authContext.HttpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>
                {
                    new Claim("oid", "token"),
                    new Claim(ClaimTypes.Role, "InvalidTestRole")
                })
            );

            // Act
            filter.OnAuthorization(authContext);

            // Assert
            var result = authContext.Result as ForbidResult;

            Assert.NotNull(result);
        }
    }
}
