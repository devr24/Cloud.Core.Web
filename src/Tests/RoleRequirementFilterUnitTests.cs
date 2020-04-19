using System.Collections.Generic;
using System.Security.Claims;
using Cloud.Core.Testing;
using Cloud.Core.Web.Filters;
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
    public class RoleRequirementFilterUnitTests
    {
        [Fact]
        public void RoleRequirementFilter_NoFeatureFlagsSupplied()
        {
            // Arrange
            var mockFeatureFlags = new Mock<IFeatureFlag>();
            var filter = new RoleRequirementFilter(new string[] { "" });

            var authContext = new AuthorizationFilterContext(
            new ActionContext
            {
                HttpContext = HttpContextMock.GetRequestHttpContext(new byte[] { }),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                    {
                    }
                }
            },
            new List<IFilterMetadata>()
            );
            authContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>
                {
                    new Claim("oid", "token")
                })
            );

            // Act
            filter.OnAuthorization(authContext);

            // Assert
            mockFeatureFlags.Verify(m => m.GetFeatureFlag(
                It.IsAny<string>(), 
                It.IsAny<bool>()
                ),
                Times.Never);
        }

        [Fact]
        public void RoleRequirementFilter_NoFeatureFlagsSupplied_ValidRoles()
        {
            // Arrange
            var mockFeatureFlags = new Mock<IFeatureFlag>();
            var filter = new RoleRequirementFilter(new string[] { "TestRole" });

            var authContext = new AuthorizationFilterContext(
            new ActionContext
            {
                HttpContext = HttpContextMock.GetRequestHttpContext(new byte[] { }),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                    {
                    }
                }
            },
            new List<IFilterMetadata>()
            );
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

        [Fact]
        public void RoleRequirementFilter_NoFeatureFlagsSupplied_InvalidRoles()
        {
            // Arrange
            var mockFeatureFlags = new Mock<IFeatureFlag>();
            var filter = new RoleRequirementFilter(new string[] { "TestRole" });

            var authContext = new AuthorizationFilterContext(
            new ActionContext
            {
                HttpContext = HttpContextMock.GetRequestHttpContext(new byte[] { }),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                    {
                    }
                }
            },
            new List<IFilterMetadata>()
            );
            authContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
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

        [Fact]
        public void RoleRequirementFilter_FeatureFlagsSupplied_RolesAuthOff()
        {
            // Arrange
            var mockFeatureFlags = new Mock<IFeatureFlag>();
            mockFeatureFlags.Setup(m => m.GetFeatureFlag(
                It.IsAny<string>(),
                It.IsAny<bool>()
                )).Returns(false);
            var filter = new RoleRequirementFilter(new string[] { "TestRole" }, mockFeatureFlags.Object);

            var authContext = new AuthorizationFilterContext(
            new ActionContext
            {
                HttpContext = HttpContextMock.GetRequestHttpContext(new byte[] { }),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                    {
                    }
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
            Assert.Null(authContext.Result);
        }

        [Fact]
        public void RoleRequirementFilter_FeatureFlagsSupplied_RolesAuthOn_ValidRoles()
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
                HttpContext = HttpContextMock.GetRequestHttpContext(new byte[] { }),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                    {
                    }
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

        [Fact]
        public void RoleRequirementFilter_FeatureFlagsSupplied_RolesAuthOn_InvalidRoles()
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
                HttpContext = HttpContextMock.GetRequestHttpContext(new byte[] { }),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                    {
                    }
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

        [Fact]
        public void RoleRequirementFilter_FeatureFlagsSupplied_RolesAuthOn_NoUser()
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
                HttpContext = HttpContextMock.GetRequestHttpContext(new byte[] { }),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                    {
                    }
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
