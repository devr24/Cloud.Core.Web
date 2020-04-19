using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Cloud.Core.Testing;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Cloud.Core.Web.Attributes;
using Cloud.Core.Web.Middlewares;

namespace Cloud.Core.Web.Tests
{
    [IsUnit]
    public class CoreWebTest
    {
        // Http Extension
        [Fact]
        public void Test_HttpExtensions_RequestToFormattedString()
        {
            // Arrange
            var fake = HttpContextMock.GetRequestHttpContext(Encoding.UTF8.GetBytes("test"));

            // Act
            var str = fake.Request.ToFormattedString();

            // Assert
            str.IndexOf("Headers: testKey:testVal", StringComparison.InvariantCulture).Should().BeGreaterThan(0);
            str.IndexOf("Hostname: localhost", StringComparison.InvariantCulture).Should().BeGreaterThan(0);
        }

        [Fact]
        public void Test_HttpExtensions_ResponseToFormattedString()
        {
            // Arrange
            var fake = HttpContextMock.GetResponseHttpContext(Encoding.UTF8.GetBytes("test"));

            // Act
            var str = fake.Response.ToFormattedString();

            // Assert
            str.IndexOf("Headers: testKey:testVal", StringComparison.InvariantCulture).Should().BeGreaterThan(0);
            str.IndexOf("Authenticated: False", StringComparison.InvariantCulture).Should().BeGreaterThan(0);
        }

        // Validate Attribute
        [Theory]
        [InlineData("test", "test cannot be tested")]
        [InlineData("attribute", "this attribute is invalid")]
        public void Test_ValidateModelAttribute_Invoke(string key, string message)
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(serviceProvider =>
                    serviceProvider.GetService(typeof(ILogger<ValidateModelAttribute>)))
                .Returns(Mock.Of<ILogger<ValidateModelAttribute>>());
            var httpContext = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            var actionExecutingContext = new ActionExecutingContext(actionContext,
                filters: new List<IFilterMetadata>(), // for majority of scenarios you need not worry about populating this parameter
                actionArguments:
                new Dictionary<string, object>(), // if the filter uses this data, add some data to this dictionary
                controller: null); // since the filter being tested here does not use the data from this parameter, just provide null

            var validationFilter = new ValidateModelAttribute();

            // Act
            actionContext.ModelState.AddModelError(key, message);
            validationFilter.OnActionExecuting(actionExecutingContext);
            var validationResult = Assert.IsType<ValidationFailedResult>(actionExecutingContext.Result);

            // Assert
            validationResult.StatusCode.Should().Be(400);

            var errorResult = Assert.IsType<ApiErrorResult>(validationResult.Value);
            errorResult.Message.Should().Be("Validation Error");
            errorResult.Errors.Count.Should().Be(1);
            errorResult.Errors[0].Message.Should().Be(message);
            errorResult.Errors[0].Field.Should().Be(key);
        }

        [Theory]
        [InlineData("test", "test cannot be tested")]
        [InlineData("attribute", "this attribute is invalid")]
        public void Test_ValidateModelAttribute_InvokeWithParam(string key, string message)
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(serviceProvider =>
                    serviceProvider.GetService(typeof(ILogger)))
                .Returns(Mock.Of<ILogger>());
            var httpContext = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            var actionExecutingContext = new ActionExecutingContext(actionContext,
                filters: new List<IFilterMetadata>(), // for majority of scenarios you need not worry about populating this parameter
                actionArguments:
                new Dictionary<string, object>(), // if the filter uses this data, add some data to this dictionary
                controller: null); // since the filter being tested here does not use the data from this parameter, just provide null

            var validationFilter = new ValidateModelAttribute(true);

            // Act
            actionContext.ModelState.AddModelError(key, message);
            validationFilter.OnActionExecuting(actionExecutingContext);
            var validationResult = Assert.IsType<ValidationFailedResult>(actionExecutingContext.Result);

            // Assert
            validationResult.StatusCode.Should().Be(400);

            var errorResult = Assert.IsType<ApiErrorResult>(validationResult.Value);
            errorResult.Message.Should().Be("Validation Error");
            errorResult.Errors.Count.Should().Be(1);
            errorResult.Errors[0].Message.Should().Be(message);
            errorResult.Errors[0].Field.Should().Be(key);
        }

        [Theory]
        [InlineData("test", "test cannot be tested")]
        [InlineData("attribute", "this attribute is invalid")]
        public void Test_ApiErrorResult_Instance(string key, string message)
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(serviceProvider =>
                    serviceProvider.GetService(typeof(ILogger<ValidateModelAttribute>)))
                .Returns(Mock.Of<ILogger<ValidateModelAttribute>>());
            var httpContext = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };

            // Act
            var err = new ApiErrorResult(new Exception(message), key);
            var servErr = new InternalServerErrorResult(new Exception(message));
            var res = httpContext.Response;
            var req = httpContext.Request;
            var apiError = new ApiError("", "");
            apiError = new ApiError(null, "");
            apiError = new ApiError("test", "");

            // Assert
            err.Message.Should().Be(key);
            err.Errors.Count.Should().Be(1);
            err.Errors[0].Message.Should().Be(message);

            servErr.StatusCode.Should().Be(500);
            Assert.IsType<ApiErrorResult>(servErr.Value);

            res.Headers.Add(key, message);
            res.StatusCode = 200;
            res.Body = new MemoryStream(Encoding.ASCII.GetBytes(message));
            res.ToFormattedString().Length.Should().BeGreaterThan(0);

            req.Headers.Add(key, message);
            req.Body = new MemoryStream(Encoding.ASCII.GetBytes(message));
            req.ToFormattedString().Length.Should().BeGreaterThan(0);
        }

        // Action context extension
        [Theory]
        [InlineData("Test1")]
        [InlineData("Test2")]
        [InlineData("Test3")]
        public void Test_ActionContextExtensions_ActionName(string actionName)
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(serviceProvider =>
                    serviceProvider.GetService(typeof(ILogger<ValidateModelAttribute>)))
                .Returns(Mock.Of<ILogger<ValidateModelAttribute>>());
            var httpContext = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };

            var actionContext = new ActionContext(httpContext, new RouteData
            {
                Values = { { "Action", actionName } }
            }, new ActionDescriptor());

            // Act
            var name = actionContext.ActionName();

            // Assert
            actionName.Should().Be(name);
        }

        [Theory]
        [InlineData("Test1")]
        [InlineData("Test2")]
        [InlineData("Test3")]
        public void Test_ActionContextExtensions_ControllerName(string controllerName)
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(serviceProvider =>
                    serviceProvider.GetService(typeof(ILogger<ValidateModelAttribute>)))
                .Returns(Mock.Of<ILogger<ValidateModelAttribute>>());
            var httpContext = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };

            var actionContext = new ActionContext(httpContext, new RouteData
            {
                Values = { { "Controller", controllerName } }
            }, new ActionDescriptor());

            // Act
            var name = actionContext.ControllerName();

            // Assert
            controllerName.Should().Be(name.Replace("Controller", string.Empty));
        }

        // Application builder extension
        [Fact]
        public async Task Test_ApplicationBuilderExtensions_UseUnhandledExceptionMiddleware()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnhandledExceptionMiddleware>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(serviceProvider =>
                    serviceProvider.GetService(typeof(ILogger<ValidateModelAttribute>)))
                .Returns(Mock.Of<ILogger<ValidateModelAttribute>>());
            var httpContext = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };
            var logRequestMiddleware = new UnhandledExceptionMiddleware(next: (innerHttpContext) => Task.FromResult(0),
                logger: loggerMock.Object);

            // Act
            await logRequestMiddleware.Invoke(httpContext);
            await logRequestMiddleware.HandleExceptionAsync(httpContext, new Exception("Test"));

            // Assert
            httpContext.Response.StatusCode.Should().Be(500);
        }

        // Application builder extension
        [Fact]
        public async Task Test_ApplicationBuilderExtensions_UseExceptionMiddleware_WithException()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(serviceProvider => serviceProvider.GetService(typeof(ILogger<ValidateModelAttribute>))).Returns(Mock.Of<ILogger<ValidateModelAttribute>>());
            var httpContext = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };
            var logRequestMiddleware = new UnhandledExceptionMiddleware(next: (innerHttpContext) => throw new Exception(), logger: null);

            // Act
            await logRequestMiddleware.Invoke(httpContext);
            await logRequestMiddleware.HandleExceptionAsync(httpContext, new Exception("Test"));

            // Assert
            httpContext.Response.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Test_ApplicationBuilderExtensions_UseExceptionMiddleware_HideSqlConnection()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(serviceProvider => serviceProvider.GetService(typeof(ILogger<ValidateModelAttribute>))).Returns(Mock.Of<ILogger<ValidateModelAttribute>>());
            var httpContext = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };
            var logRequestMiddleware = new UnhandledExceptionMiddleware(next: (innerHttpContext) => throw new Exception("Cannot open server"), logger: null);

            // Act
            await logRequestMiddleware.Invoke(httpContext);
            await logRequestMiddleware.HandleExceptionAsync(httpContext, new Exception("Test"));

            // Assert
            httpContext.Response.StatusCode.Should().Be(500);
        }

        [Fact]
        public void Test_ValidationFailedResult_ToString()
        {
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("TestKey1", "TestValue1");
            modelState.AddModelError("TestKey2", "TestValue2");

            var res = new ValidationFailedResult(modelState);

            var str = res.ToString();
            str.Should().Be("Validation Error (400): Model could not be validated. TestKey1 - TestValue1, TestKey2 - TestValue2");
        }

        [Fact]
        public void Test_ValidationFailedResult_ToStringEmpty()
        {
            var modelState = new ModelStateDictionary();

            var res = new ValidationFailedResult(modelState);

            var str = res.ToString();
            str.Should().Be("Validation Error (400): Model could not be validated.");
        }

        [Fact]
        public async Task Test_ApplicationBuilderExtensions_AddHealthProbe()
        {
            var webBuilder = WebHost.CreateDefaultBuilder(null).UseStartup<FakeStartup>();
            webBuilder.UseUrls(FakeStartup.ADDRESS);
            var host = webBuilder.Build();
            host.Start();

            var httpClient = host.Services.GetService<IHttpClientFactory>();

            var res = await httpClient.CreateClient("default").GetAsync("probe");
            res.StatusCode.Should().Be(200);
        }

        // Http Context extension
        [Fact]
        public void Test_HttpContextExtensions_GetRequestHeader()
        {
            // Arrange 
            var fakeRequest = HttpContextMock.GetRequestHttpContext(Encoding.UTF8.GetBytes("test"));

            // Act
            var header = fakeRequest.GetRequestHeader("testKey");
            var emptyHeader = fakeRequest.GetRequestHeader("doesnotexist");

            // Assert
            header.Should().Be("testVal");
            emptyHeader.Should().BeNullOrEmpty();
        }

        [Fact]
        public void Test_HttpContextExtensions_GetClaim()
        {
            // Arrange 
            var fakeRequest = HttpContextMock.GetResponseHttpContext(Encoding.UTF8.GetBytes("test"));
            var actionContext = new ActionContext(fakeRequest, new RouteData(), new ActionDescriptor());

            // Assert
            actionContext.HttpContext.Response.HttpContext.GetClaimValue<string>("testKey").Should().Be("testVal");
            actionContext.HttpContext.Response.HttpContext.GetClaimValue<string>("doesnotexist").Should().Be(null);
        }

        [Fact]
        public void Test_HttpContextExtensions_GetConnectionInfo()
        {
            // Arrange 
            var fakeRequest = HttpContextMock.GetResponseHttpContext(Encoding.UTF8.GetBytes("test"));
            var actionContext = new ActionContext(fakeRequest, new RouteData(), new ActionDescriptor());
            var defaultAddress = IPAddress.Parse("127.0.0.1").ToString();

            // Act
            var clientIp = actionContext.HttpContext.Response.HttpContext.GetClientIpAddress();

            // Assert
            clientIp.Should().Be(defaultAddress);
        }
    }

    public class FakeStartup
    {
        public const string ADDRESS = "http://127.0.0.1:8085/";
        public FakeStartup(IConfiguration configuration) { }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient("default", client => client.BaseAddress = new Uri(ADDRESS));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.AddHealthProbe();
            app.UseUnhandledExceptionMiddleware();
        }
    }
}
