namespace System.Reflection
{
    /// <summary>
    /// Class AssemblyExtensions.
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Gets the name of the assembly passed in.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>System.String.</returns>
        internal static string GetAssemblyName(this Assembly assembly)
        {
            return assembly?.GetName().Name;
        }
    }
}
