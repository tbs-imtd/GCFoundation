namespace GCFoundation.Components.Models
{
    /// <summary>
    /// Represents a group of related tag helper references.
    /// </summary>
    public class TagHelperReferenceGroupViewModel
    {
        /// <summary>
        /// Group title used in the collapsible section.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Tag helper entries that belong to this group.
        /// </summary>
        public List<TagHelperReferenceViewModel> Items { get; set; } = new List<TagHelperReferenceViewModel>();
    }
}
