using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Moq;

namespace Cloud.Core.Web.Tests
{
    public class HttpContextMock
    {
        public static HttpContext GetRequestHttpContext(byte[] contentBytes, string contentType = "application/json")
        {
            var request = new Mock<HttpRequest>();
            
            request.SetupGet(f => f.Body).Returns(new MemoryStream(contentBytes));
            request.SetupGet(f => f.ContentType).Returns(contentType);
            request.Setup(x => x.Scheme).Returns("http");
            request.Setup(x => x.HttpContext.Connection.RemoteIpAddress).Returns(Dns.GetHostEntry("127.0.0.1").AddressList[0]);
            request.Setup(x => x.Headers).Returns(new HeaderDictionary() { new KeyValuePair<string, StringValues>("testKey", new StringValues("testVal")) });
            request.Setup(x => x.Host).Returns(new HostString("localhost"));
            request.Setup(x => x.Path).Returns(new PathString("/test"));
            request.Setup(x => x.PathBase).Returns(new PathString("/"));
            request.Setup(x => x.Method).Returns("GET");
            request.Setup(x => x.QueryString).Returns(new QueryString("?param1=2"));

            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            return httpContext.Object;
        }

        public static HttpContext GetResponseHttpContext(byte[] contentBytes, string contentType = "application/json")
        {
            var response = new Mock<HttpResponse>();
            var headers = new Mock<IHeaderDictionary>();
            headers.Object.Add("test", "test");

            response.Setup(x => x.Headers).Returns(new HeaderDictionary() { new KeyValuePair<string, StringValues>("testKey", new StringValues("testVal")) });
            response.SetupGet(f => f.Body).Returns(new MemoryStream(contentBytes));
            response.SetupGet(f => f.ContentType).Returns(contentType);
            response.SetupGet(f => f.HttpContext.Connection.RemoteIpAddress).Returns(IPAddress.Parse("127.0.0.1"));
            response.SetupGet(f => f.HttpContext.User).Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
            {
                new ClaimsIdentity(new List<Claim>{ new Claim("testKey","testVal") })
            }));

            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.Response).Returns(response.Object);
            return httpContext.Object;
        }
    }
}
