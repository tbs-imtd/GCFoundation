using GCFoundation.Components.Models;

namespace GCFoundation.Web.Models.Components
{
    /// <summary>
    /// ViewModel representing a component.
    /// </summary>
    public class ComponentViewModel : BaseViewModel
    {
        /// <summary>
        /// List of accessibility requirements (DO).
        /// </summary>
        public IEnumerable<string> AccessibilityDo { get; set; } = new List<string>();

        /// <summary>
        /// List of accessibility requirements (DO NOT).
        /// </summary>
        public IEnumerable<string> AccessibilityDoNot { get; set; } = new List<string>();

        /// <summary>
        /// (Optional) Other names the component is known by.
        /// </summary>
        public string AlsoCalled { get; set; } = string.Empty;

        /// <summary>
        /// (Optional) Additional guidance sections (e.g. anatomy, colour meaning) rendered after the standard guidance.
        /// </summary>
        public IEnumerable<ComponentSampleCodeSectionViewModel> GuidanceSections { get; set; } = new List<ComponentSampleCodeSectionViewModel>();

        /// <summary>
        /// Name of the component.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// List of notes regarding the component.
        /// </summary>
        public IEnumerable<string> Notes { get; set; } = new List<string>();

        /// <summary>
        /// Introduction paragraph to present the component and it's purpose or recommended use.
        /// </summary>
        public string Overview { get; set; } = string.Empty;

        /// <summary>
        /// List of properties that can be used with the component.
        /// </summary>
        public IEnumerable<ComponentPropertyViewModel> Properties { get; set; } = new List<ComponentPropertyViewModel>();

        /// <summary>
        /// Brief description of the component's purpose.
        /// </summary>
        public string Purpose { get; set; } = string.Empty;

        /// <summary>
        /// List of sample code sections for the component.
        /// </summary>
        public IEnumerable<ComponentSampleCodeSectionViewModel> SampleCodeSections { get; set; } = new List<ComponentSampleCodeSectionViewModel>();

        /// <summary>
        /// Should the view display a side navigation or not?
        /// </summary>
        public bool ShowSideNavigation { get; set; } = true;

        private SideNavigationViewModel? sideNavigation;
        /// <summary>
        /// (Optional) Model representing the list of links to the sections of the page for the side navigation.
        /// </summary>
        public SideNavigationViewModel? SideNavigation
        {
            get
            {
                if (sideNavigation == null)
                    return DefaultSideNavigation();
                else
                    return sideNavigation;
            }
            set
            {
                sideNavigation = value;
            }
        }

        /// <summary>
        /// Html tag (to be used by developers) that will render the component.
        /// </summary>
        public string Tag { get; set; } = string.Empty;

        /// <summary>
        /// List of UX/UI best practices.
        /// </summary>
        public IEnumerable<string> UxBestPractices { get; set; } = new List<string>();

        /// <summary>
        /// List of scenarios when NOT to use this component.
        /// </summary>
        public IEnumerable<string> WhenNotToUse { get; set; } = new List<string>();

        /// <summary>
        /// (Optional) Note displayed after the list of scenarios when NOT to use this component.
        /// </summary>
        public string WhenNotToUseNote { get; set; } = string.Empty;

        /// <summary>
        /// List of scenarios when to use this component.
        /// </summary>
        public IEnumerable<string> WhenToUse { get; set; } = new List<string>();

        /// <summary>
        /// (Optional) Common examples displayed after the list of scenarios when to use this component.
        /// </summary>
        public IEnumerable<string> WhenToUseExamples { get; set; } = new List<string>();


        /// <summary>
        /// Builds default side navigation based on the current properties of the ViewModel (i.e. include links to the Overview, SampleCodeSections, Properties and Notes).
        /// </summary>
        /// <returns></returns>
        private SideNavigationViewModel DefaultSideNavigation()
        {
            var items = new List<NavItem>();

            if (!string.IsNullOrEmpty(Overview))
                items.Add(new NavLink() { Href = Resources.Components.Overview_Anchor, Label = Resources.Components.Overview });
            if (!string.IsNullOrEmpty(Purpose))
                items.Add(new NavLink() { Href = Resources.Components.Guidance_Purpose_Anchor, Label = Resources.Components.Guidance_Purpose });
            if (WhenToUse != null && WhenToUse.Any())
                items.Add(new NavLink() { Href = Resources.Components.Guidance_WhenToUse_Anchor, Label = Resources.Components.Guidance_WhenToUse });
            if (WhenNotToUse != null && WhenNotToUse.Any())
                items.Add(new NavLink() { Href = Resources.Components.Guidance_WhenNotToUse_Anchor, Label = Resources.Components.Guidance_WhenNotToUse });
            if ((AccessibilityDo != null && AccessibilityDo.Any()) || (AccessibilityDoNot != null && AccessibilityDoNot.Any()))
                items.Add(new NavLink() { Href = Resources.Components.Guidance_Accessibility_Anchor, Label = Resources.Components.Guidance_Accessibility });
            if (UxBestPractices != null && UxBestPractices.Any())
                items.Add(new NavLink() { Href = Resources.Components.Guidance_UxBestPractices_Anchor, Label = Resources.Components.Guidance_UxBestPractices });
            if (GuidanceSections != null && GuidanceSections.Any())
                items.AddRange(GuidanceSections.Select(s => new NavLink() { Href = s.Id, Label = s.Title }).ToList());
            if (SampleCodeSections != null && SampleCodeSections.Any())
                items.AddRange(SampleCodeSections.Select(s => new NavLink() { Href = s.Id, Label = s.Title }).ToList());
            if (Properties != null && Properties.Any())
                items.Add(new NavLink() { Href = Resources.Components.Properties_Anchor, Label = Resources.Components.Properties });
            if (Notes != null && Notes.Any())
                items.Add(new NavLink() { Href = Resources.Components.Notes_Anchor, Label = Resources.Components.Notes });

            return new SideNavigationViewModel() { Items = items };
        }
    }
}