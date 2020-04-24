using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace Cloud.Core.Web.Tests.Fakes
{
    /// <summary>
    /// Class FakeInputFormatterContext.
    /// Implements the <see cref="Microsoft.AspNetCore.Mvc.Formatters.InputFormatterContext" />
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Formatters.InputFormatterContext" />
    public class FakeInputFormatterContext : InputFormatterContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FakeInputFormatterContext"/> class.
        /// </summary>
        /// <param name="httpContext">The <see cref="T:Microsoft.AspNetCore.Http.HttpContext" /> for the current operation.</param>
        /// <param name="modelName">The name of the model.</param>
        /// <param name="modelState">The <see cref="T:Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary" /> for recording errors.</param>
        /// <param name="metadata">The <see cref="T:Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata" /> of the model to deserialize.</param>
        /// <param name="readerFactory">A delegate which can create a <see cref="T:System.IO.TextReader" /> for the request body.</param>
        public FakeInputFormatterContext(HttpContext httpContext, string modelName, ModelStateDictionary modelState, ModelMetadata metadata, Func<Stream, Encoding, TextReader> readerFactory) : base(httpContext, modelName, modelState, metadata, readerFactory)
        {
        }
    }

    /// <summary>
    /// Class FakeOutputFormatterContext.
    /// Implements the <see cref="Microsoft.AspNetCore.Mvc.Formatters.OutputFormatterWriteContext" />
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Formatters.OutputFormatterWriteContext" />
    public class FakeOutputFormatterContext : OutputFormatterWriteContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FakeOutputFormatterContext"/> class.
        /// </summary>
        /// <param name="httpContext">The <see cref="T:Microsoft.AspNetCore.Http.HttpContext" /> for the current request.</param>
        /// <param name="writerFactory">The delegate used to create a <see cref="T:System.IO.TextWriter" /> for writing the response.</param>
        /// <param name="objectType">The <see cref="T:System.Type" /> of the object to write to the response.</param>
        /// <param name="object">The object to write to the response.</param>
        public FakeOutputFormatterContext(HttpContext httpContext, Func<Stream, Encoding, TextWriter> writerFactory, Type objectType, object @object)
            : base(httpContext, writerFactory, objectType, @object)
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="P:Microsoft.AspNetCore.Mvc.Formatters.OutputFormatterCanWriteContext.HttpContext" /> context associated with the current operation.
        /// </summary>
        /// <value>The HTTP context.</value>
        public override HttpContext HttpContext { get => base.HttpContext; protected set => base.HttpContext = value; }
        /// <summary>
        /// Gets or sets the content type to write to the response.
        /// </summary>
        /// <value>The type of the content.</value>
        /// <remarks>An <see cref="T:Microsoft.AspNetCore.Mvc.Formatters.IOutputFormatter" /> can set this value when its
        /// <see cref="M:Microsoft.AspNetCore.Mvc.Formatters.IOutputFormatter.CanWriteResult(Microsoft.AspNetCore.Mvc.Formatters.OutputFormatterCanWriteContext)" /> method is called,
        /// and expect to see the same value provided in
        /// <see cref="M:Microsoft.AspNetCore.Mvc.Formatters.IOutputFormatter.WriteAsync(Microsoft.AspNetCore.Mvc.Formatters.OutputFormatterWriteContext)" /></remarks>
        public override StringSegment ContentType { get => base.ContentType; set => base.ContentType = value; }
        /// <summary>
        /// Gets or sets a value to indicate whether the content type was specified by server-side code.
        /// This allows <see cref="M:Microsoft.AspNetCore.Mvc.Formatters.IOutputFormatter.CanWriteResult(Microsoft.AspNetCore.Mvc.Formatters.OutputFormatterCanWriteContext)" /> to
        /// implement stricter filtering on content types that, for example, are being considered purely
        /// because of an incoming Accept header.
        /// </summary>
        /// <value><c>true</c> if [content type is server defined]; otherwise, <c>false</c>.</value>
        public override bool ContentTypeIsServerDefined { get => base.ContentTypeIsServerDefined; set => base.ContentTypeIsServerDefined = value; }
        /// <summary>
        /// Gets or sets the object to write to the response.
        /// </summary>
        /// <value>The object.</value>
        public override object Object { get => base.Object; protected set => base.Object = value; }
        /// <summary>
        /// Gets or sets the <see cref="T:System.Type" /> of the object to write to the response.
        /// </summary>
        /// <value>The type of the object.</value>
        public override Type ObjectType { get => base.ObjectType; protected set => base.ObjectType = value; }
        /// <summary>
        /// <para>
        /// Gets or sets a delegate used to create a <see cref="T:System.IO.TextWriter" /> for writing text to the response.
        /// </para>
        /// <para>
        /// Write to <see cref="P:Microsoft.AspNetCore.Http.HttpResponse.Body" /> directly to write binary data to the response.
        /// </para>
        /// </summary>
        /// <value>The writer factory.</value>
        /// <remarks><para>
        /// The <see cref="T:System.IO.TextWriter" /> created by this delegate will encode text and write to the
        /// <see cref="P:Microsoft.AspNetCore.Http.HttpResponse.Body" /> stream. Call this delegate to create a <see cref="T:System.IO.TextWriter" />
        /// for writing text output to the response stream.
        /// </para>
        /// <para>
        /// To implement a formatter that writes binary data to the response stream, do not use the
        /// <see cref="P:Microsoft.AspNetCore.Mvc.Formatters.OutputFormatterWriteContext.WriterFactory" /> delegate, and use <see cref="P:Microsoft.AspNetCore.Http.HttpResponse.Body" /> instead.
        /// </para></remarks>
        public override Func<Stream, Encoding, TextWriter> WriterFactory { get => base.WriterFactory; protected set => base.WriterFactory = value; }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
