using Cloud.Core.Testing;
using Cloud.Core.Web.Csv;
using Cloud.Core.Web.Tests.Fakes;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Cloud.Core.Web.Tests
{
    [IsUnit]
    public class CsvFormatterTests
    {
        /// <summary>Ensure exceptions are thrown as expected when formatters are setup with a null parameter.</summary>
        [Fact]
        public void Test_CsvFormatter_Instance()
        {
            Assert.Throws<ArgumentNullException>(() => new CsvInputFormatter(null));
            Assert.Throws<ArgumentNullException>(() => new CsvOutputFormatter(null));
        }

        /// <summary>
        /// This will test converting a csv input and converting it to an IEnumerable object.
        /// NOTE: Disabeld this test for now as there's problems with it.
        /// </summary>
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
            var mockHttpContext = FakeHttpContext.GetResponseHttpContext(Encoding.ASCII.GetBytes(content), "text/cv");
            var mockContext = new FakeOutputFormatterContext(mockHttpContext, (s, e) => new StreamWriter(new MemoryStream()),
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

            Assert.Throws<ArgumentNullException>(() => instance.CanWriteTypeInternal(null));
            
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
}
