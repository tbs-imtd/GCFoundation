namespace GCFoundation.Common.Settings
{
    /// <summary>
    /// The pinned version of one CDN package and the Subresource Integrity hashes of the files GCFoundation loads from it.
    /// </summary>
    public sealed class CdnPackageIntegrity
    {
        /// <summary>
        /// Gets the npm package name.
        /// </summary>
        public string Package { get; init; } = string.Empty;

        /// <summary>
        /// Gets the pinned package version.
        /// </summary>
        public string Version { get; init; } = string.Empty;

        /// <summary>
        /// Gets the hash of the package's stylesheet.
        /// </summary>
        public string Css { get; init; } = string.Empty;

        /// <summary>
        /// Gets the hash of the package's JavaScript entry point, if GCFoundation loads one.
        /// </summary>
        public string JavaScript { get; init; } = string.Empty;
    }
}
