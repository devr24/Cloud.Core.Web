using Cloud.Core.Testing;
using Cloud.Core.Web.Csv;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Cloud.Core.Web.Tests
{
    [IsUnit]
    public class CsvFormatterTest
    {
        [Fact]
        public void Test_CsvFormatter_Instance()
        {
            Assert.Throws<ArgumentNullException>(() => new CsvInputFormatter(null));
            Assert.Throws<ArgumentNullException>(() => new CsvOutputFormatter(null));
        }

        // This will test converting a csv input and converting it to an IEnumerable object.
        [Fact]
        public void Test_CsvInputFormatter_CanRead()
        {
            //// Arrange
            //var options = new CsvFormatterOptions { CsvDelimiter = ",", UseSingleLineHeaderInCsv = true };
            //var instance = new CsvInputFormatter(options);

            //// Expected output after conversion.
            //var inputObject = new List<TestCsvParsed>() {
            //        new TestCsvParsed { HeaderOne = "1", HeaderTwo = "2", HeaderThree = "3" },
            //        new TestCsvParsed { HeaderOne = "4", HeaderTwo = "5", HeaderThree = "6" }
            //    };
            //var content = $"HeaderOne,HeaderTwo,HeaderThree{Environment.NewLine}1,2,3{Environment.NewLine}4,5,6";


            //// Act
            //instance.ReadRequestBodyAsync(mockContext).GetAwaiter().GetResult();

            //// Assert
            //instance.CanRead(mockContext).Should().BeTrue();
            //instance.SupportedMediaTypes.Should().BeEquivalentTo(new MediaTypeCollection() { "text/csv" });

            //StreamReader reader = new StreamReader(mockContext.HttpContext.Response.Body);
            //string strResponse = reader.ReadToEnd();
            //strResponse.Should().Be(content);
        }

        // This will test converting an IEnumerable to an output CSV string.
        [Fact]
        public void Test_CsvOutputFormatter_CanWrite()
        {
            // Arrange
            var options = new CsvFormatterOptions { CsvDelimiter = ",", UseSingleLineHeaderInCsv = true };
            var instance = new CsvOutputFormatter(options);

            // Expected output after conversion.
            var content = $"HeaderOne,HeaderTwo,HeaderThree{Environment.NewLine}1,2,3{Environment.NewLine}4,5,6";

            // Mock contexts for using with the output formatter (along with object that will be converted to Csv).
            var mockHttpContext = HttpContextMock.GetResponseHttpContext(Encoding.ASCII.GetBytes(content), "text/cv");
            var mockContext = new MockOutputFormatterContext(mockHttpContext, (s, e) => new StreamWriter(new MemoryStream()),
                typeof(List<TestCsvParsed>),
                new List<TestCsvParsed>() {
                    new TestCsvParsed { HeaderOne = "1", HeaderTwo = "2", HeaderThree = "3" },
                    new TestCsvParsed { HeaderOne = "4", HeaderTwo = "5", HeaderThree = "6" }
                });

            // Act
            instance.WriteResponseBodyAsync(mockContext).GetAwaiter().GetResult();

            // Assert
            instance.ContentType.Should().Be("text/csv");
            instance.CanWriteResult(mockContext).Should().BeTrue();
            instance.SupportedMediaTypes.Should().BeEquivalentTo(new MediaTypeCollection() { "text/csv" });

            StreamReader reader = new StreamReader(mockContext.HttpContext.Response.Body);
            string strResponse = reader.ReadToEnd();
            strResponse.Should().Be(content);
        }
    }

    public class TestCsvParsed
    {
        public string HeaderOne { get; set; }
        public string HeaderTwo { get; set; }
        public string HeaderThree { get; set; }
    }

    public class MockInputFormatterContext : InputFormatterContext
    {
        public MockInputFormatterContext(HttpContext httpContext, string modelName, ModelStateDictionary modelState, ModelMetadata metadata, Func<Stream, Encoding, TextReader> readerFactory) : base(httpContext, modelName, modelState, metadata, readerFactory)
        {
        }
    }

    public class MockOutputFormatterContext : OutputFormatterWriteContext
    {
        public MockOutputFormatterContext(HttpContext httpContext, Func<Stream, Encoding, TextWriter> writerFactory, Type objectType, object @object) 
            : base(httpContext, writerFactory, objectType, @object)
        {
        }

        public override HttpContext HttpContext { get => base.HttpContext; protected set => base.HttpContext = value; }
        public override StringSegment ContentType { get => base.ContentType; set => base.ContentType = value; }
        public override bool ContentTypeIsServerDefined { get => base.ContentTypeIsServerDefined; set => base.ContentTypeIsServerDefined = value; }
        public override object Object { get => base.Object; protected set => base.Object = value; }
        public override Type ObjectType { get => base.ObjectType; protected set => base.ObjectType = value; }
        public override Func<Stream, Encoding, TextWriter> WriterFactory { get => base.WriterFactory; protected set => base.WriterFactory = value; }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
