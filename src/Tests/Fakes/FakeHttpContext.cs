using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;

namespace Cloud.Core.Web.Tests.Fakes
{
    /// <summary>
    /// Class Fake Http Context.
    /// </summary>
    public class FakeHttpContext
    {
        /// <summary>
        /// Gets the request HTTP context.
        /// </summary>
        /// <param name="contentBytes">The content bytes.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>HttpContext fake.</returns>
        public static HttpContext GetRequestHttpContext(byte[] contentBytes, string contentType = "application/json")
        {
            ClaimsPrincipal user;
            var request = new Mock<HttpRequest>();
            
            request.SetupGet(f => f.Body).Returns(new MemoryStream(contentBytes));
            request.SetupGet(f => f.ContentType).Returns(contentType);
            request.Setup(x => x.Scheme).Returns("http");
            request.Setup(x => x.HttpContext.Connection.RemoteIpAddress).Returns(Dns.GetHostEntry("127.0.0.1").AddressList.FirstOrDefault());
            request.Setup(x => x.Headers).Returns(new HeaderDictionary() { 
                new KeyValuePair<string, StringValues>("testKey", new StringValues("testVal")),
                new KeyValuePair<string, StringValues>("referer", new StringValues("http://www.fakeaddress.com")),
            });
            request.Setup(x => x.Host).Returns(new HostString("localhost"));
            request.Setup(x => x.Path).Returns(new PathString("/test"));
            request.Setup(x => x.PathBase).Returns(new PathString("/"));
            request.Setup(x => x.Method).Returns("GET");
            request.Setup(x => x.QueryString).Returns(new QueryString("?param1=2"));

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim("oid", "TestUser"));
            user = new ClaimsPrincipal(identity);

            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            httpContext.SetupGet(c => c.User).Returns(() => user);
            httpContext.SetupSet(c => c.User = It.IsAny<ClaimsPrincipal>())
                .Callback<ClaimsPrincipal>(c => user = c);
            httpContext.SetupProperty(p => p.RequestServices);

            return httpContext.Object;
        }

        /// <summary>
        /// Gets the response HTTP context.
        /// </summary>
        /// <param name="contentBytes">The content bytes.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="status">The status.</param>
        /// <returns>HttpContext fake.</returns>
        public static HttpContext GetResponseHttpContext(byte[] contentBytes, string contentType = "application/json", HttpStatusCode status = HttpStatusCode.OK)
        {
            var response = new Mock<Microsoft.AspNetCore.Http.HttpResponse>();
            response.SetupGet(f => f.StatusCode).Returns((int)status);
            response.SetupGet(f => f.Headers).Returns(new HeaderDictionary() { new KeyValuePair<string, StringValues>("testKey", new StringValues("testVal")) });
            response.SetupGet(f => f.Body).Returns(() => {

                var memStream = new MemoryStream();
                memStream.Write(contentBytes);

                var streamWriter = new StreamWriter(memStream);
                streamWriter.Flush();
                streamWriter.BaseStream.Position = 0;
                return streamWriter.BaseStream;
            });
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
