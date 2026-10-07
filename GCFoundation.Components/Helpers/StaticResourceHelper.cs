namespace GCFoundation.Components.Helpers
{
    /// <summary>
    /// Provides utility methods to retrieve the path of static resources (CSS, JS, images, libraries) in the project.
    /// </summary>
    public static class StaticResourceHelper
    {
        private static string _virtualDirectoryPrefix = string.Empty;
        // Cache-busting key for static web assets. These files are served from `/_content/...` and
        // can be aggressively cached by browsers/CDNs; using a version-based query string ensures
        // clients fetch updated CSS/JS when the components package version changes.
        private static readonly string _staticAssetsVersion =
            typeof(StaticResourceHelper).Assembly.GetName().Version?.ToString() ?? "1";

        /// <summary>
        /// Configures the virtual directory prefix used when building resource paths.
        /// </summary>
        /// <param name="virtualDirectoryName">The virtual directory name, with or without leading slash.</param>
        public static void ConfigureVirtualDirectoryPrefix(string? virtualDirectoryName)
        {
            if (string.IsNullOrWhiteSpace(virtualDirectoryName))
            {
                _virtualDirectoryPrefix = string.Empty;
                return;
            }

            var trimmed = virtualDirectoryName.Trim().Trim('/');
            _virtualDirectoryPrefix = string.IsNullOrEmpty(trimmed) ? string.Empty : "/" + trimmed;
        }

        /// <summary>
        /// Gets the full resource path for a given resource relative path.
        /// </summary>
        /// <param name="resourceRelativePath">The relative path to the resource.</param>
        /// <returns>A string representing the full path to the resource.</returns>
        public static string GetResourcePath(string resourceRelativePath)
        {
            string entryAssemblyName = "GCFoundation.Components";
            return $"{_virtualDirectoryPrefix}/_content/{entryAssemblyName}/{resourceRelativePath}?v={_staticAssetsVersion}";
        }

        /// <summary>
        /// Gets the full path for a given CSS file.
        /// </summary>
        /// <param name="cssFile">The name of the CSS file.</param>
        /// <returns>A string representing the full path to the CSS file.</returns>
        public static string GetCssResourcePath(string cssFile)
        {
            return GetResourcePath($"css/{cssFile}");
        }

        /// <summary>
        /// Gets the full path for a given JavaScript file.
        /// </summary>
        /// <param name="jsFile">The name of the JavaScript file.</param>
        /// <returns>A string representing the full path to the JavaScript file.</returns>
        public static string GetJsResourcePath(string jsFile)
        {
            return GetResourcePath($"js/{jsFile}");
        }

        /// <summary>
        /// Gets the full path for a given image file.
        /// </summary>
        /// <param name="imageFile">The name of the image file.</param>
        /// <returns>A string representing the full path to the image file.</returns>
        public static string GetImageResourcePath(string imageFile)
        {
            return GetResourcePath($"images/{imageFile}");
        }

        /// <summary>
        /// Gets the full path for a given library file.
        /// </summary>
        /// <param name="libFile">The name of the library file.</param>
        /// <returns>A string representing the full path to the library file.</returns>
        public static string GetLibResourcePath(string libFile)
        {
            return GetResourcePath($"lib/{libFile}");
        }
    }
}
