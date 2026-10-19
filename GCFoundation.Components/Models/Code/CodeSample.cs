namespace GCFoundation.Components.Models.Code
{
    /// <summary>
    /// One language variant of a code example rendered by <c>&lt;fdcp-code-block&gt;</c>,
    /// <c>&lt;fdcp-code-example&gt;</c> or <see cref="Helpers.FDCPCodeRenderer"/>.
    /// </summary>
    public class CodeSample
    {
        /// <summary>
        /// Stable language identifier, such as <c>csharp</c>, <c>javascript</c>, <c>python</c> or <c>json</c>.
        /// It selects the Prism grammar and is the value synchronized between examples. Unknown or empty
        /// identifiers are displayed as plain text.
        /// </summary>
        public string Language { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the language, such as "C#". When omitted, a label is derived from <see cref="Language"/>.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Original source text. It is always treated as text and HTML-encoded once when rendered.
        /// </summary>
        public string Code { get; set; } = string.Empty;
    }
}
