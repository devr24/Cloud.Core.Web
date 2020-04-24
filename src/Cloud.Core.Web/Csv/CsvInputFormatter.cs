namespace Cloud.Core.Web.Csv
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Formatters;

    /// <summary>
    /// Allow web requests to accept Csv formatted input (instead of json).  Useful for Csv import.
    /// </summary>
    /// <seealso cref="InputFormatter" />
    [ExcludeFromCodeCoverage] // Need to test this later
    public class CsvInputFormatter : InputFormatter
    {
        private readonly CsvFormatterOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvInputFormatter"/> class.
        /// </summary>
        /// <param name="csvFormatterOptions">The CSV formatter options.</param>
        /// <exception cref="ArgumentNullException">csvFormatterOptions</exception>
        public CsvInputFormatter(CsvFormatterOptions csvFormatterOptions)
        {
            SupportedMediaTypes.Add(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("text/csv"));
            _options = csvFormatterOptions ?? throw new ArgumentNullException(nameof(csvFormatterOptions));
        }

        /// <summary>
        /// Reads an object from the request body.
        /// </summary>
        /// <param name="context">The <see cref="T:Microsoft.AspNetCore.Mvc.Formatters.InputFormatterContext" />.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that on completion deserializes the request body.</returns>
        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var type = context.ModelType;
            var request = context.HttpContext.Request;
            MediaTypeHeaderValue.TryParse(request.ContentType, out _);

            var result = ReadStream(type, request.Body);
            return InputFormatterResult.SuccessAsync(result);
        }

        /// <summary>
        /// Determines whether this instance can read the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if this instance can read the specified context; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">context - Model type not set</exception>
        /// <inheritdoc />
        public override bool CanRead(InputFormatterContext context)
        {
            var type = context.ModelType;
            if (type == null)
            {
                throw new ArgumentNullException(nameof(context), "Model type not set");
            }

            return IsTypeOfIEnumerable(type);
        } 

        private bool IsTypeOfIEnumerable(Type type)
        {
            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType == typeof(IList))
                {
                    return true;
                }
            }

            return false;
        }

        private object ReadStream(Type type, Stream stream)
        {
            Type itemType;
            var typeIsArray = false;
            IList list;
            if (type.GetGenericArguments().Length > 0)
            {
                itemType = type.GetGenericArguments()[0];
                list = (IList)Activator.CreateInstance(type);
            }
            else
            {
                typeIsArray = true;
                itemType = type.GetElementType();

                var listType = typeof(List<>);
                var constructedListType = listType.MakeGenericType(itemType);

                list = (IList)Activator.CreateInstance(constructedListType);
            }

            var reader = new StreamReader(stream);

            bool skipFirstLine = _options.UseSingleLineHeaderInCsv;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line?.Split(_options.CsvDelimiter.ToCharArray());
                if (skipFirstLine)
                {
                    skipFirstLine = false;
                }
                else
                {
                    var itemTypeInGeneric = list.GetType().GetTypeInfo().GenericTypeArguments[0];
                    var item = Activator.CreateInstance(itemTypeInGeneric);
                    var properties = item.GetType().GetProperties();
                    for (int i = 0; i < values?.Length; i++)
                    {
                        properties[i].SetValue(item, Convert.ChangeType(values[i], properties[i].PropertyType), null);
                    }

                    list.Add(item);
                }

            }

            if (typeIsArray)
            {
                var iType = itemType ?? typeof(Array);
                var array = Array.CreateInstance(iType, list.Count);

                for (int t = 0; t < list.Count; t++)
                {
                    array.SetValue(list[t], t);
                }
                return array;
            }

            return list;
        }
    }
}
