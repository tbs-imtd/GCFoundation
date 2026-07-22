namespace GCFoundation.Web.Models.Components
{
    /// <summary>
    /// ViewModel representing all content required to render the Components index page.
    /// </summary>
    public class ComponentsIndexPageViewModel
    {
        /// <summary>
        /// Featured component cards shown at the top of the page.
        /// </summary>
        public List<ComponentIndexViewModel> FeaturedComponents { get; set; } = new List<ComponentIndexViewModel>();

        /// <summary>
        /// Grouped references for non-featured tag helpers.
        /// </summary>
        public List<TagHelperReferenceGroupViewModel> TagHelperGroups { get; set; } = new List<TagHelperReferenceGroupViewModel>();
    }
}
