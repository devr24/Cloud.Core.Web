namespace Cloud.Core.Web.Csv
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Formatters;

    /// <summary>
    /// Export classes as Csv by adding the following attribute to the top of your controller method:
    /// [Produces("text/csv")]
    /// </summary>
    /// <seealso cref="OutputFormatter" />
    public class CsvOutputFormatter : OutputFormatter
    {
        private readonly CsvFormatterOptions _options;

        public string ContentType { get; private set; }

        public CsvOutputFormatter(CsvFormatterOptions csvFormatterOptions)
        {
            ContentType = "text/csv";
            SupportedMediaTypes.Add(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("text/csv"));
            _options = csvFormatterOptions ?? throw new ArgumentNullException(nameof(csvFormatterOptions), "Formatter options must be set");
        }

        protected override bool CanWriteType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.IsEnumerableType();
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            if (context.HttpContext.Response.StatusCode != (int)HttpStatusCode.OK)
                return;

            var type = context.Object.GetType();
            Type itemType;

            if (type.GetGenericArguments().Length > 0)
                itemType = type.GetGenericArguments()[0];
            else
                itemType = type.GetElementType();

            if (itemType == null)
                throw new NotSupportedException("Object type must be enumerable");

            var stringWriter = new StringWriter();

            if (_options.UseSingleLineHeaderInCsv)
                await stringWriter.WriteLineAsync(string.Join<string>(_options.CsvDelimiter, itemType.GetProperties().Select(x => x.Name)));

            foreach (var obj in (IEnumerable<object>)context.Object)
            {
                var vals = obj.GetType().GetProperties().Select(pi => new { Value = pi.GetValue(obj, null) });

                string valueLine = string.Empty;

                foreach (var val in vals)
                {
                    if (val.Value != null)
                    {
                        var actualValue = val.Value.ToString();

                        // Check if the value contains a comma and place it in quotes if so
                        if (actualValue.Contains(","))
                            actualValue = string.Concat("\"", actualValue, "\"");

                        // Replace any \r or \n special characters from a new line with a space
                        if (actualValue.Contains("\r"))
                            actualValue = actualValue.Replace("\r", " ");

                        if (actualValue.Contains("\n"))
                            actualValue = actualValue.Replace("\n", " ");

                        valueLine = string.Concat(valueLine, actualValue, _options.CsvDelimiter);
                    }
                    else
                        valueLine = string.Concat(valueLine, string.Empty, _options.CsvDelimiter);
                }

                await stringWriter.WriteLineAsync(valueLine.TrimEnd(_options.CsvDelimiter.ToCharArray()));
            }

            var streamWriter = new StreamWriter(context.HttpContext.Response.Body);
            await streamWriter.WriteAsync(stringWriter.ToString());
            await streamWriter.FlushAsync();
        }
    }

    public class CsvFormatterOptions
    {
        public bool UseSingleLineHeaderInCsv { get; set; } = true;

        public string CsvDelimiter { get; set; } = ";";
    }
}
