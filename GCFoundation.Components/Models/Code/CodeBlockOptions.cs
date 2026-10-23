using GCFoundation.Components.Enums;

namespace GCFoundation.Components.Models.Code
{
    /// <summary>
    /// Options for rendering a code component with <see cref="Helpers.FDCPCodeRenderer"/>.
    /// </summary>
    public class CodeBlockOptions
    {
        /// <summary>
        /// Gets or sets an accessible name for the code when the surrounding content already shows a visible
        /// heading and <see cref="Title"/> is omitted. The language name is appended automatically.
        /// </summary>
        public string? AccessibleLabel { get; set; }

        /// <summary>
        /// Gets or sets whether the copy button is rendered. Defaults to <c>true</c>.
        /// </summary>
        public bool CopyEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets additional CSS classes for the component root.
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the component root. Invalid characters are replaced and a unique
        /// identifier is generated when omitted.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the language variants. One sample renders without a language selector; several samples
        /// render a selector. Language identifiers must be unique within one component.
        /// </summary>
        public IReadOnlyList<CodeSample> Samples { get; set; } = [];

        /// <summary>
        /// Gets or sets the language shown first. Defaults to the first sample when omitted or not available.
        /// </summary>
        public string? SelectedLanguage { get; set; }

        /// <summary>
        /// Gets or sets an optional group name. Components sharing a group on the same page switch language together.
        /// </summary>
        public string? SyncGroup { get; set; }

        /// <summary>
        /// Gets or sets the colour theme. <see cref="FDCPCodeTheme.dark"/> uses a dark background and
        /// <c>color-scheme: dark</c> so Dark Reader does not invert the syntax colours. Defaults to light.
        /// </summary>
        public FDCPCodeTheme Theme { get; set; } = FDCPCodeTheme.light;

        /// <summary>
        /// Gets or sets an optional visible title for the code.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets whether long lines wrap instead of scrolling horizontally. Defaults to <c>false</c>.
        /// </summary>
        public bool Wrap { get; set; }
    }
}
