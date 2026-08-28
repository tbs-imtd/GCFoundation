using System.Collections.ObjectModel;

namespace GCFoundation.Web.Models
{
    /// <summary>
    /// Represents a group of related tag helper references.
    /// </summary>
    public class TagHelperReferenceGroupViewModel
    {
        /// <summary>
        /// Tag helper entries that belong to this group.
        /// </summary>
        public Collection<TagHelperReferenceViewModel> Items { get; } = new Collection<TagHelperReferenceViewModel>();

        /// <summary>
        /// Group title used in the collapsible section.
        /// </summary>
        public string Title { get; set; } = string.Empty;
    }
}