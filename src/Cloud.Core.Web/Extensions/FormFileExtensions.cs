namespace Microsoft.AspNetCore.Http
{
    using System.IO;

    /// <summary>
    /// Form file extensions.
    /// </summary>
    public static class FormFileExtensions
    {
        /// <summary>
        /// Gets the form file extension.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="removeFullstop">If set to <c>true</c> [remove fullstop from file extension].</param>
        /// <returns>System.String.</returns>
        public static string GetExtension(this IFormFile file, bool removeFullstop = true)
        {
            if (file == null || file.FileName.Length == 0)
                return null;

            // Get the file extension and remove the full stop.
            var ext = Path.GetExtension(file.FileName);
            if (ext.Length > 0 && removeFullstop)
                ext = ext.Replace(".", string.Empty);

            return ext;
        }

        /// <summary>
        /// Gets the form file name without extension.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>System.String.</returns>
        public static string GetFileNameWithoutExtension(this IFormFile file)
        {
            return Path.GetFileNameWithoutExtension(file.FileName);
        }
    }
}
