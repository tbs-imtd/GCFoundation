namespace GCFoundation.Components.Enums
{
    /// <summary>
    /// Colour theme for <c>&lt;fdcp-code&gt;</c>.
    /// </summary>
    public enum FDCPCodeTheme
    {
        /// <summary>
        /// Light background. This is the default.
        /// </summary>
        light = 0,

        /// <summary>
        /// Dark background. The component sets <c>color-scheme: dark</c> so Dark Reader does not invert it.
        /// </summary>
        dark = 1
    }
}
