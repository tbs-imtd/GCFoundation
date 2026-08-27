namespace GCFoundation.Components.Models
{
    /// <summary>
    /// Represents a tag helper reference entry displayed on the Components index page.
    /// </summary>
    public class TagHelperReferenceViewModel
    {
        /// <summary>
        /// Display title for the tag helper entry.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Short description of what the tag helper is used for.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Core attributes commonly used with the tag helper.
        /// </summary>
        public List<string> KeyProperties { get; set; } = new List<string>();

        /// <summary>
        /// Minimal usage snippet for quick reference.
        /// </summary>
        public string UsageSnippet { get; set; } = string.Empty;
    }
}
