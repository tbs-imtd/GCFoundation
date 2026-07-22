using GCFoundation.Components.Controllers;
using GCFoundation.Components.Enums;
using GCFoundation.Components.Models.FormBuilder;
using GCFoundation.Web.Models;
using GCFoundation.Web.Models.Components;
using GCFoundation.Web.Resources;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace GCFoundation.Web.Controllers
{
    /// <summary>
    /// Controller that handles requests related to reusable UI components.
    /// </summary>
    [Route("components")]
    public class ComponentsController(ILogger<ComponentsController> logger) : GCFoundationBaseController(logger)
    {
        /// <summary>
        /// Displays the main components overview page.
        /// </summary>
        /// <returns>
        /// The components index view.
        /// </returns>
        [HttpGet("")]
        public IActionResult Index()
        {
            SetPageTitle(Menu.Menu_Components);

            var vm = BuildComponentsIndexPageViewModel();
            return View(vm);
        }

        /// <summary>
        /// Displays the Badge component demo page.
        /// </summary>
        /// <returns>
        /// The Badge component view.
        /// </returns>
        [HttpGet("badge")]
        public IActionResult Badge()
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_Badge_Title}");

            var vm = BuildBadgeComponentViewModel();
            return View("Component", vm);
        }

        /// <summary>
        /// Displays the Card component demo page.
        /// </summary>
        /// <returns>
        /// The Card component view.
        /// </returns>
        [HttpGet("card")]
        public IActionResult Card()
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_Card_Title}");

            var vm = BuildCardComponentViewModel();
            return View("Component", vm);
        }

        /// <summary>
        /// Displays the Filtered Search component demo page.
        /// </summary>
        /// <returns>
        /// The Filtered Search component view.
        /// </returns>
        [HttpGet("filtered-search")]
        public IActionResult FilteredSearch()
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_FilteredSearch_Title}");

            var vm = BuildFilteredSearchComponentViewModel();
            return View("Component", vm);
        }

        /// <summary>
        /// Demonstrates a comprehensive example of a dynamic form with various question types and dependencies.
        /// This example showcases all possible dependency actions and their interactions.
        /// </summary>
        /// <returns>
        /// A view containing a form with various input types and complex dependencies.
        /// </returns>
        [HttpGet("form-builder")]
        public IActionResult FormBuilder()
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_FormBuilder_Title}");

            var vm = BuildFormBuilderTestViewModel();
            return View("FormBuilder", vm);
        }

        /// <summary>
        /// Handles the submission of the dynamic form builder example.
        /// Validates the form data and processes it if valid.
        /// </summary>
        /// <param name="vm">The view model containing form definition and user input.</param>
        /// <returns>
        /// Redirects to the example form builder view with a success message if valid; otherwise, returns the form view with validation errors.
        /// </returns>
        [HttpPost("form-builder")]
        [ValidateAntiForgeryToken]
        public IActionResult FormBuilder([FromForm] FormViewModel vm)
        {
            ArgumentNullException.ThrowIfNull(vm, nameof(vm));

            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_FormBuilder_Title}");

            var fbvm = BuildFormBuilderTestViewModel(vm);

            // Add the form data to the validation context
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(vm.Form)
            {
                Items = { ["FormData"] = vm.FormData }
            };

            // Validate the model including dependencies
            if (!TryValidateModel(vm, nameof(FormViewModel)))
            {
                // If validation fails, return to the form with error messages
                return View("FormBuilder", fbvm);
            }

            // Process the valid form data
            // TODO: Add your form processing logic here

            // Redirect to success page or show success message
            fbvm.SuccessMessage = Resources.Components.FormBuilder_SampleForm_SubmittedSuccessfully;
            return View("FormBuilder", fbvm);
        }

        /// <summary>
        /// Displays a sample form linked to properties of a class.
        /// </summary>
        /// <returns>
        /// The Form component view.
        /// </returns>
        [HttpGet("form")]
        public IActionResult Form()
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_Form_Title}");

            var vm = BuildFormComponentViewModel();
            return View("Form", vm);
        }

        /// <summary>
        /// Handles the POST request to test form validation.
        /// </summary>
        /// <param name="vm">The form data submitted by the user.</param>
        /// <returns>
        /// The Form component view with the POSTed model and validation results.
        /// </returns>
        [HttpPost("form")]
        [ValidateAntiForgeryToken]
        public IActionResult Form(FormTestViewModel vm)
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_Form_Title}");

            vm = BuildFormComponentViewModel(vm);
            if (ModelState.IsValid)
            {
                // Add your logic here if needed when the model is valid
                vm.SuccessMessage = Resources.Components.Form_SampleForm_SubmittedSuccessfully;
            }

            return View("Form", vm);
        }

        /// <summary>
        /// Displays the GC Design System components page.
        /// </summary>
        /// <returns>
        /// The GC Design System view.
        /// </returns>
        [HttpGet("gcds")]
        public IActionResult Gcds()
        {
            SetPageTitle(Menu.Menu_Components_GCDesign);

            return View();
        }

        /// <summary>
        /// Displays the Modal component demo page.
        /// </summary>
        /// <returns>
        /// The Modal component view.
        /// </returns>
        [HttpGet("modal")]
        public IActionResult Modal()
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_Modal_Title}");

            var vm = BuildModalComponentViewModel();
            return View("Modal", vm);
        }

        /// <summary>
        /// Displays the PageHeading component demo page.
        /// </summary>
        /// <returns>
        /// The PageHeading component view.
        /// </returns>
        [HttpGet("page-heading")]
        public IActionResult PageHeading()
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_PageHeading_Title}");

            var vm = BuildPageHeadingComponentViewModel();
            return View("Component", vm);
        }

        /// <summary>
        /// Displays the Searchable Select component demo page.
        /// </summary>
        /// <returns>
        /// The Searchable Select component view.
        /// </returns>
        [HttpGet("searchable-select")]
        public IActionResult SearchableSelect()
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_SearchableSelect_Title}");

            var vm = BuildSearchableSelectComponentViewModel();
            return View("Component", vm);
        }

        /// <summary>
        /// Displays the Stepper component demo page.
        /// </summary>
        /// <returns>
        /// The Stepper component view.
        /// </returns>
        [HttpGet("stepper")]
        public IActionResult Stepper()
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_Stepper_Title}");

            var vm = BuildStepperComponentViewModel();
            return View("Component", vm);
        }

        /// <summary>
        /// Displays the standard table component demo page.
        /// </summary>
        /// <returns>The Table component view.</returns>
        [HttpGet("table")]
        public IActionResult Table()
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_Table_Title}");
            var vm = BuildTableComponentViewModel();
            return View("Component", vm);
        }

        /// <summary>
        /// Displays the User Login Partial component demo page.
        /// </summary>
        /// <returns>
        /// The User Login Partial component view.
        /// </returns>
        [HttpGet("user-login")]
        public IActionResult UserLogin()
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_UserLoginPartial_Title}");
            ViewData["LoginPartialViewName"] = "_ExampleUserLogin";
            return View();
        }


        #region ViewModel Building
        private static ComponentViewModel BuildBadgeComponentViewModel()
        {
            var vm = new ComponentViewModel();

            vm.Name = Resources.Components.Badge_Name;
            vm.Notes = new List<string>()
            {
                Resources.Components.Badge_Notes_1,
                Resources.Components.Badge_Notes_2,
                Resources.Components.Badge_Notes_3
            };
            vm.Overview = Resources.Components.Badge_Overview;
            vm.Purpose = Resources.Components.Badge_Purpose;
            vm.WhenToUse = new List<string>()
            {
                Resources.Components.Badge_WhenToUse_1,
                Resources.Components.Badge_WhenToUse_2,
                Resources.Components.Badge_WhenToUse_3
            };
            vm.WhenNotToUse = new List<string>()
            {
                Resources.Components.Badge_WhenNotToUse_1,
                Resources.Components.Badge_WhenNotToUse_2,
                Resources.Components.Badge_WhenNotToUse_3
            };
            vm.AccessibilityDo = new List<string>()
            {
                Resources.Components.Badge_Accessibility_Do_1,
                Resources.Components.Badge_Accessibility_Do_2,
                Resources.Components.Badge_Accessibility_Do_3,
                Resources.Components.Badge_Accessibility_Do_4,
                Resources.Components.Badge_Accessibility_Do_5
            };
            vm.AccessibilityDoNot = new List<string>()
            {
                Resources.Components.Badge_Accessibility_DoNot_1,
                Resources.Components.Badge_Accessibility_DoNot_2
            };
            vm.UxBestPractices = new List<string>()
            {
                Resources.Components.Badge_UxBestPractices_1,
                Resources.Components.Badge_UxBestPractices_2,
                Resources.Components.Badge_UxBestPractices_3,
                Resources.Components.Badge_UxBestPractices_4
            };
            vm.Properties = new List<ComponentPropertyViewModel>()
            {
                new ComponentPropertyViewModel() { Name = "style", DataType = "FDCPBadgeStyle", Description = Resources.Components.Badge_Properties_Style },
                new ComponentPropertyViewModel() { Name = "inverted", DataType = "bool", Description = Resources.Components.Badge_Properties_Inverted },
                new ComponentPropertyViewModel() { Name = "start-content", DataType = "string", Description = Resources.Components.Badge_Properties_StartContent },
                new ComponentPropertyViewModel() { Name = "end-content", DataType = "string", Description = Resources.Components.Badge_Properties_EndContent },
                new ComponentPropertyViewModel() { Name = "tag-id", DataType = "string", Description = Resources.Components.Badge_Properties_TagId }
            };
            vm.SampleCodeSections = new List<ComponentSampleCodeSectionViewModel>()
            {
                new ComponentSampleCodeSectionViewModel() { Description = Resources.Components.Badge_Solid_Text, Id = Resources.Components.Badge_Solid_Anchor, PartialViewName = "Badge/_Solid", Title = Resources.Components.Badge_Solid_Title },
                new ComponentSampleCodeSectionViewModel() { Description = Resources.Components.Badge_Inverted_Text, Id = Resources.Components.Badge_Inverted_Anchor, PartialViewName = "Badge/_Inverted", Title = Resources.Components.Badge_Inverted_Title },
                new ComponentSampleCodeSectionViewModel() { Description = Resources.Components.Badge_Slot_Text, Id = Resources.Components.Badge_Slot_Anchor, PartialViewName = "Badge/_Slot", Title = Resources.Components.Badge_Slot_Title }
            };
            vm.Tag = "<fdcp-badge>";

            return vm;
        }
        private static ComponentViewModel BuildCardComponentViewModel()
        {
            var vm = new ComponentViewModel();

            vm.Name = Resources.Components.Card_Name;
            vm.Notes = new List<string>()
            {
                Resources.Components.Card_Notes_1,
                Resources.Components.Card_Notes_2,
                Resources.Components.Card_Notes_3,
                Resources.Components.Card_Notes_4
            };
            vm.Overview = Resources.Components.Card_Overview;
            vm.Properties = new List<ComponentPropertyViewModel>()
            {
                new ComponentPropertyViewModel() { Name = "tag-id", DataType = "string", Description = Resources.Components.Card_Properties_TagId },
                new ComponentPropertyViewModel() { Name = "width", DataType = "string", Description = Resources.Components.Card_Properties_Width },
                new ComponentPropertyViewModel() { Name = "height", DataType = "string", Description = Resources.Components.Card_Properties_Height },
                new ComponentPropertyViewModel() { Name = "border", DataType = "bool", Description = Resources.Components.Card_Properties_Border },
                new ComponentPropertyViewModel() { Name = "shadow", DataType = "bool", Description = Resources.Components.Card_Properties_Shadow },
                new ComponentPropertyViewModel() { Name = "image-top", DataType = "string", Description = Resources.Components.Card_Properties_ImageTop },
                new ComponentPropertyViewModel() { Name = "image-bottom", DataType = "string", Description = Resources.Components.Card_Properties_ImageBottom },
                new ComponentPropertyViewModel() { Name = "image-alt", DataType = "string", Description = Resources.Components.Card_Properties_ImageAlt },
                new ComponentPropertyViewModel() { Name = "horizontal", DataType = "bool", Description = Resources.Components.Card_Properties_Horizontal }
            };
            vm.SampleCodeSections = new List<ComponentSampleCodeSectionViewModel>()
            {
                new ComponentSampleCodeSectionViewModel() { Description = Resources.Components.Card_Basic_Text, Id = Resources.Components.Card_Basic_Anchor, PartialViewName = "Card/_Basic", Title = Resources.Components.Card_Basic_Title },
                new ComponentSampleCodeSectionViewModel() { Description = Resources.Components.Card_Horizontal_Text, Id = Resources.Components.Card_Horizontal_Anchor, PartialViewName = "Card/_Horizontal", Title = Resources.Components.Card_Horizontal_Title },
                new ComponentSampleCodeSectionViewModel() { Description = Resources.Components.Card_WithImages_Text, Id = Resources.Components.Card_WithImages_Anchor, PartialViewName = "Card/_WithImages", Title = Resources.Components.Card_WithImages_Title },
                new ComponentSampleCodeSectionViewModel() { Description = Resources.Components.Card_WithSlots_Text, Id = Resources.Components.Card_WithSlots_Anchor, PartialViewName = "Card/_WithSlots", Title = Resources.Components.Card_WithSlots_Title }
            };
            vm.Tag = "<fdcp-card>";

            return vm;
        }
        private static ComponentViewModel BuildFilteredSearchComponentViewModel()
        {
            var vm = new ComponentViewModel();

            vm.Name = Resources.Components.FilteredSearch_Name;
            //vm.Notes = new List<string>()
            //{
            //    Resources.Components.FilteredSearch_Notes_1,
            //    Resources.Components.FilteredSearch_Notes_2,
            //    Resources.Components.FilteredSearch_Notes_3
            //};
            vm.Overview = Resources.Components.FilteredSearch_Overview;
            vm.Properties = new List<ComponentPropertyViewModel>()
            {
                new ComponentPropertyViewModel() { Name = "title", DataType = "string", Description = Resources.Components.FilteredSearch_Properties_Title },
                new ComponentPropertyViewModel() { Name = "filters", DataType = "IEnumerable<SearchFilterCategory>", Description = Resources.Components.FilteredSearch_Properties_Filters }
            };
            vm.SampleCodeSections = new List<ComponentSampleCodeSectionViewModel>()
            {
                new ComponentSampleCodeSectionViewModel() { Id = Resources.Components.FilteredSearch_Basic_Anchor, PartialViewName = "FilteredSearch/_Basic", Title = Resources.Components.FilteredSearch_Basic_Title }
            };
            vm.Tag = "<fdcp-filters-box>";

            return vm;
        }
        private FormBuilderTestViewModel BuildFormBuilderTestViewModel(FormViewModel? vm = null)
        {
            var fbvm = new FormBuilderTestViewModel();
            if (vm != null)
                fbvm.SampleFormBuilder = vm;
            else
                fbvm.SampleFormBuilder = new FormViewModel() { Form = GenerateSampleFormDefinition() };

            fbvm.Name = Resources.Components.FormBuilder_Name;
            fbvm.Properties = new List<ComponentPropertyViewModel>()
            {
                new ComponentPropertyViewModel() { Name = "form", DataType = "GCFoundation.Components.Models.FormBuilder.FormDefinition", Description = Resources.Components.FormBuilder_Properties_Form }            };
            fbvm.SampleCodeSections = new List<ComponentSampleCodeSectionViewModel>()
            {
                new ComponentSampleCodeSectionViewModel() { Id = Resources.Components.FormBuilder_SampleForm_Anchor, Description = Resources.Components.FormBuilder_SampleForm_Description, Title = Resources.Components.FormBuilder_SampleForm_Title },
            };
            fbvm.Tag = "<fdcp-form-builder>";

            return fbvm;
        }
        private static FormTestViewModel BuildFormComponentViewModel(FormTestViewModel? vm = null)
        {
            if (vm == null)
                vm = new FormTestViewModel();

            vm.Name = Resources.Components.Form_Name;
            //vm.Notes = new List<string>()
            //{
            //    Resources.Components.Form_Notes_1,
            //    Resources.Components.Form_Notes_2,
            //    Resources.Components.Form_Notes_3
            //};
            vm.Overview = Resources.Components.Form_Overview;
            vm.Properties = new List<ComponentPropertyViewModel>()
            {
                new ComponentPropertyViewModel() { Name = "for", DataType = "GCFoundation.Components.Models.BaseViewModel", Description = Resources.Components.Form_Properties_For },
                new ComponentPropertyViewModel() { Name = "method", DataType = "string", DefaultValue = "POST", Description = Resources.Components.Form_Properties_Method },
                new ComponentPropertyViewModel() { Name = "action", DataType = "string", Description = Resources.Components.Form_Properties_Action }
            };
            vm.SampleCodeSections = new List<ComponentSampleCodeSectionViewModel>()
            {
                new ComponentSampleCodeSectionViewModel() { Id = Resources.Components.Form_SampleForm_Anchor, Title = Resources.Components.Form_SampleForm_Title },
            };
            vm.Tag = "<fdcp-form>";

            return vm;
        }
        private ComponentsIndexPageViewModel BuildComponentsIndexPageViewModel()
        {
            return new ComponentsIndexPageViewModel()
            {
                FeaturedComponents = BuildFeaturedComponentCards(),
                TagHelperGroups = BuildTagHelperReferenceGroups()
            };
        }
        private List<ComponentIndexViewModel> BuildFeaturedComponentCards()
        {
            var vm = new List<ComponentIndexViewModel>()
            {
                new () { Name = Resources.Components.Index_Badge_Title, Description = Resources.Components.Index_Badge_Description, Href = Url.Action("Badge", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-badge.svg") },
                new () { Name = Resources.Components.Index_Card_Title, Description = Resources.Components.Index_Card_Description, Href = Url.Action("Card", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-card.svg") },
                new () { Name = Resources.Components.Index_FilteredSearch_Title, ShortDescription = Resources.Components.Index_FilteredSearch_Description, Href = Url.Action("FilteredSearch", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-filtered-search.svg") },
                new () { Name = Resources.Components.Index_FormBuilder_Title, ShortDescription = Resources.Components.Index_FormBuilder_Description, Href = Url.Action("FormBuilder", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-form-builder.svg") },
                new () { Name = Resources.Components.Index_Form_Title, ShortDescription = Resources.Components.Index_Form_Description, Href = Url.Action("Form", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-form.svg") },
                new () { Name = Resources.Components.Index_Modal_Title, ShortDescription = Resources.Components.Index_Modal_Description, Href = Url.Action("Modal", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-modal.svg") },
                new () { Name = Resources.Components.Index_PageHeading_Title, ShortDescription = Resources.Components.Index_PageHeading_Description, Href = Url.Action("PageHeading", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-page-heading.svg") },
                new () { Name = Resources.Components.Index_SearchableSelect_Title, ShortDescription = Resources.Components.Index_SearchableSelect_Description, Href = Url.Action("SearchableSelect", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-select.svg") },
                new () { Name = Resources.Components.Index_Stepper_Title, ShortDescription = Resources.Components.Index_Stepper_Description, Href = Url.Action("Stepper", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-stepper-fdcp.svg") },
                new () { Name = Resources.Components.Index_Table_Title, ShortDescription = Resources.Components.Index_Table_Description, Href = Url.Action("Table", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-table.svg") },
                new () { Name = Resources.Components.Index_UserLoginPartial_Title, ShortDescription = Resources.Components.Index_UserLoginPartial_Description, Href = Url.Action("UserLogin", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-user-login-partial.svg") }
            };
            return vm;
        }
        private static List<TagHelperReferenceGroupViewModel> BuildTagHelperReferenceGroups()
        {
            return new List<TagHelperReferenceGroupViewModel>()
            {
                new TagHelperReferenceGroupViewModel()
                {
                    Title = GetComponentResourceString("Index_TagHelpers_Group_FDCP_Title"),
                    Items = BuildFdcpTagHelperReferences()
                },
                new TagHelperReferenceGroupViewModel()
                {
                    Title = GetComponentResourceString("Index_TagHelpers_Group_GCDS_Title"),
                    Items = BuildGcdsTagHelperReferences()
                }
            };
        }
        private static List<TagHelperReferenceViewModel> BuildFdcpTagHelperReferences()
        {
            return new List<TagHelperReferenceViewModel>()
            {
                new() { Title = "<fdcp-checkbox>", Description = GetComponentResourceString("Index_TagHelpers_Description_FDCP_CheckboxSingle"), KeyProperties = new List<string>() { "for", "name", "label", "required" }, UsageSnippet = "<fdcp-checkbox for=\"Model.AcceptTerms\" label=\"Accept terms\" />" },
                new() { Title = "<fdcp-checkboxes>", Description = GetComponentResourceString("Index_TagHelpers_Description_FDCP_CheckboxGroup"), KeyProperties = new List<string>() { "for", "items", "name", "legend" }, UsageSnippet = "<fdcp-checkboxes for=\"Model.SelectedValues\" items=\"Model.AvailableOptions\" />" },
                new() { Title = "<fdcp-error-summary>", Description = GetComponentResourceString("Index_TagHelpers_Description_FDCP_Feedback"), KeyProperties = new List<string>() { "for", "title", "compact" }, UsageSnippet = "<fdcp-error-summary for=\"Model\" />" },
                new() { Title = "<fdcp-input>", Description = GetComponentResourceString("Index_TagHelpers_Description_FDCP_FormField"), KeyProperties = new List<string>() { "for", "name", "label", "type" }, UsageSnippet = "<fdcp-input for=\"Model.Email\" type=\"email\" />" },
                new() { Title = "<fdcp-radios>", Description = GetComponentResourceString("Index_TagHelpers_Description_FDCP_FormField"), KeyProperties = new List<string>() { "for", "items", "name", "legend" }, UsageSnippet = "<fdcp-radios for=\"Model.Selection\" items=\"Model.Options\" />" },
                new() { Title = "<fdcp-rich-text>", Description = GetComponentResourceString("Index_TagHelpers_Description_FDCP_Editor"), KeyProperties = new List<string>() { "for", "name", "height", "toolbar" }, UsageSnippet = "<fdcp-rich-text for=\"Model.ProjectSummary\" height=\"260px\" />" },
                new() { Title = "<fdcp-select>", Description = GetComponentResourceString("Index_TagHelpers_Description_FDCP_FormField"), KeyProperties = new List<string>() { "for", "items", "name", "label" }, UsageSnippet = "<fdcp-select for=\"Model.Province\" items=\"Model.ProvinceOptions\" />" },
                new() { Title = "<fdcp-session-modal>", Description = GetComponentResourceString("Index_TagHelpers_Description_FDCP_Session"), KeyProperties = new List<string>() { "id", "logout-url", "refresh-url", "session-timeout" }, UsageSnippet = "<fdcp-session-modal id=\"session-timeout-modal\" session-timeout=\"900\" />" },
                new() { Title = "<fdcp-table-gridjs>", Description = GetComponentResourceString("Index_TagHelpers_Description_FDCP_Data"), KeyProperties = new List<string>() { "id", "src", "caption", "columns" }, UsageSnippet = "<fdcp-table-gridjs id=\"employees-grid\" src=\"/api/employees\" />" }
            };
        }
        private static List<TagHelperReferenceViewModel> BuildGcdsTagHelperReferences()
        {
            return new List<TagHelperReferenceViewModel>()
            {
                new() { Title = "<gcds-breadcrumbs>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Navigation"), KeyProperties = new List<string>() { "label", "hide-on-mobile" }, UsageSnippet = "<gcds-breadcrumbs label=\"Breadcrumb navigation\"></gcds-breadcrumbs>" },
                new() { Title = "<gcds-button>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Content"), KeyProperties = new List<string>() { "type", "button-role", "size", "variant" }, UsageSnippet = "<gcds-button type=\"submit\">Submit</gcds-button>" },
                new() { Title = "<gcds-checkboxes>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_FormField"), KeyProperties = new List<string>() { "name", "legend", "hint", "required" }, UsageSnippet = "<gcds-checkboxes name=\"topics\" legend=\"Select topics\"></gcds-checkboxes>" },
                new() { Title = "<gcds-container>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Layout"), KeyProperties = new List<string>() { "size", "centered", "padding" }, UsageSnippet = "<gcds-container size=\"xl\">Content</gcds-container>" },
                new() { Title = "<gcds-date-input>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_FormField"), KeyProperties = new List<string>() { "name", "legend", "hint", "required" }, UsageSnippet = "<gcds-date-input name=\"birth-date\" legend=\"Date of birth\"></gcds-date-input>" },
                new() { Title = "<gcds-date-modified>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_DateModified"), KeyProperties = new List<string>() { "type", "display-date" }, UsageSnippet = "<gcds-date-modified>2026-07-22</gcds-date-modified>" },
                new() { Title = "<gcds-details>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Details"), KeyProperties = new List<string>() { "details-title", "open", "name" }, UsageSnippet = "<gcds-details details-title=\"More information\">Content</gcds-details>" },
                new() { Title = "<gcds-error-message>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Feedback"), KeyProperties = new List<string>() { "message", "id" }, UsageSnippet = "<gcds-error-message message=\"Field is required\"></gcds-error-message>" },
                new() { Title = "<gcds-error-summary>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Feedback"), KeyProperties = new List<string>() { "heading", "message" }, UsageSnippet = "<gcds-error-summary heading=\"Please fix the following\"></gcds-error-summary>" },
                new() { Title = "<gcds-fieldset>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_FormField"), KeyProperties = new List<string>() { "legend", "hint", "required" }, UsageSnippet = "<gcds-fieldset legend=\"Contact information\"></gcds-fieldset>" },
                new() { Title = "<gcds-file-upload>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_FormField"), KeyProperties = new List<string>() { "name", "label", "accept", "multiple" }, UsageSnippet = "<gcds-file-upload name=\"attachments\" label=\"Upload files\"></gcds-file-upload>" },
                new() { Title = "<gcds-grid>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Layout"), KeyProperties = new List<string>() { "columns", "columns-tablet", "columns-desktop", "equal-row-height" }, UsageSnippet = "<gcds-grid columns=\"1fr\" columns-desktop=\"repeat(2, 1fr)\"></gcds-grid>" },
                new() { Title = "<gcds-heading>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Typography"), KeyProperties = new List<string>() { "tag", "character-limit", "margin-top", "margin-bottom" }, UsageSnippet = "<gcds-heading tag=\"h2\">Section title</gcds-heading>" },
                new() { Title = "<gcds-icon>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Content"), KeyProperties = new List<string>() { "name", "margin-right", "margin-left" }, UsageSnippet = "<gcds-icon name=\"checkmark\"></gcds-icon>" },
                new() { Title = "<gcds-input>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_FormField"), KeyProperties = new List<string>() { "for", "label", "hint", "required" }, UsageSnippet = "<gcds-input for=\"Model.FirstName\"></gcds-input>" },
                new() { Title = "<gcds-lang-toggle>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Navigation"), KeyProperties = new List<string>() { "lang", "href" }, UsageSnippet = "<gcds-lang-toggle href=\"/fr/components\"></gcds-lang-toggle>" },
                new() { Title = "<gcds-link>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Content"), KeyProperties = new List<string>() { "href", "display", "size" }, UsageSnippet = "<gcds-link href=\"/help\">Help</gcds-link>" },
                new() { Title = "<gcds-notice>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Feedback"), KeyProperties = new List<string>() { "notice-type", "notice-title" }, UsageSnippet = "<gcds-notice notice-type=\"info\" notice-title=\"Heads up\"></gcds-notice>" },
                new() { Title = "<gcds-pagination>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Navigation"), KeyProperties = new List<string>() { "total-pages", "current-page", "url-template" }, UsageSnippet = "<gcds-pagination total-pages=\"10\" current-page=\"1\"></gcds-pagination>" },
                new() { Title = "<gcds-radios>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_FormField"), KeyProperties = new List<string>() { "name", "legend", "hint", "required" }, UsageSnippet = "<gcds-radios name=\"contact-method\" legend=\"Preferred contact method\"></gcds-radios>" },
                new() { Title = "<gcds-search>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Search"), KeyProperties = new List<string>() { "action", "method", "label" }, UsageSnippet = "<gcds-search action=\"/search\" method=\"get\"></gcds-search>" },
                new() { Title = "<gcds-select>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_FormField"), KeyProperties = new List<string>() { "name", "label", "hint", "required" }, UsageSnippet = "<gcds-select name=\"province\" label=\"Province\"></gcds-select>" },
                new() { Title = "<gcds-signature>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Content"), KeyProperties = new List<string>() { "type", "variant", "lang" }, UsageSnippet = "<gcds-signature type=\"organization\"></gcds-signature>" },
                new() { Title = "<gcds-sr-only>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Utilities"), KeyProperties = new List<string>() { "tag" }, UsageSnippet = "<gcds-sr-only>Screen-reader only text</gcds-sr-only>" },
                new() { Title = "<gcds-text>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Typography"), KeyProperties = new List<string>() { "size", "character-limit", "margin-top", "margin-bottom" }, UsageSnippet = "<gcds-text>Body copy content.</gcds-text>" },
                new() { Title = "<gcds-textarea>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_FormField"), KeyProperties = new List<string>() { "name", "label", "hint", "required" }, UsageSnippet = "<gcds-textarea name=\"comments\" label=\"Comments\"></gcds-textarea>" },
                new() { Title = "<gcds-topic-menu>", Description = GetComponentResourceString("Index_TagHelpers_Description_GCDS_Navigation"), KeyProperties = new List<string>() { "menu-title" }, UsageSnippet = "<gcds-topic-menu menu-title=\"On this page\"></gcds-topic-menu>" }
            };
        }
        private static string GetComponentResourceString(string key)
        {
            return Resources.Components.ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
        }
        private static ComponentViewModel BuildModalComponentViewModel()
        {
            var vm = new ComponentViewModel();

            vm.Name = Resources.Components.Modal_Name;
            vm.Notes = new List<string>()
            {
                Resources.Components.Modal_Notes_1,
                Resources.Components.Modal_Notes_2,
                Resources.Components.Modal_Notes_3,
                Resources.Components.Modal_Notes_4,
                Resources.Components.Modal_Notes_5
            };
            vm.Overview = Resources.Components.Modal_Overview;
            vm.Properties = new List<ComponentPropertyViewModel>()
            {
                new ComponentPropertyViewModel() { Name = "id", DataType = "string", DefaultValue = "modal", Description = Resources.Components.Modal_Properties_Id },
                new ComponentPropertyViewModel() { Name = "title", DataType = "string", DefaultValue = "Modal Title", Description = Resources.Components.Modal_Properties_Title },
                new ComponentPropertyViewModel() { Name = "centered", DataType = "bool", DefaultValue = "true", Description = Resources.Components.Modal_Properties_Centered },
                new ComponentPropertyViewModel() { Name = "scrollable", DataType = "bool", Description = Resources.Components.Modal_Properties_Scrollable },
                new ComponentPropertyViewModel() { Name = "size", DataType = "ModalSize", DefaultValue = "ModalSize.Default", Description = Resources.Components.Modal_Properties_Size },
                new ComponentPropertyViewModel() { Name = "show-close-button", DataType = "bool", DefaultValue = "true", Description = Resources.Components.Modal_Properties_ShowCloseButton },
                new ComponentPropertyViewModel() { Name = "is-static-backdrop", DataType = "bool", Description = Resources.Components.Modal_Properties_IsStaticBackdrop },
                new ComponentPropertyViewModel() { Name = "session-timeout", DataType = "int", Description = Resources.Components.Modal_Properties_SessionTimeout },
                new ComponentPropertyViewModel() { Name = "reminder-time", DataType = "int", Description = Resources.Components.Modal_Properties_ReminderTime },
                new ComponentPropertyViewModel() { Name = "refresh-url", DataType = "Uri", Description = Resources.Components.Modal_Properties_RefreshUrl },
                new ComponentPropertyViewModel() { Name = "logout-url", DataType = "Uri", Description = Resources.Components.Modal_Properties_LogoutUrl }
            };
            vm.SampleCodeSections = new List<ComponentSampleCodeSectionViewModel>()
            {
                new ComponentSampleCodeSectionViewModel() { Description = Resources.Components.Modal_Basic_Text, Id = Resources.Components.Modal_Basic_Anchor, PartialViewName = "Modal/_Basic", Title = Resources.Components.Modal_Basic_Title },
                new ComponentSampleCodeSectionViewModel() { Description = Resources.Components.Modal_Session_Text, Id = Resources.Components.Modal_Session_Anchor, PartialViewName = "Modal/_Session", Title = Resources.Components.Modal_Session_Title }
            };
            vm.Tag = "<fdcp-modal>";

            return vm;
        }
        private static ComponentViewModel BuildPageHeadingComponentViewModel()
        {
            var vm = new ComponentViewModel();

            vm.Name = Resources.Components.PageHeading_Name;
            vm.Notes = new List<string>()
            {
                Resources.Components.PageHeading_Notes_1,
                Resources.Components.PageHeading_Notes_2,
                Resources.Components.PageHeading_Notes_3
            };
            vm.Overview = Resources.Components.PageHeading_Overview;
            vm.Properties = new List<ComponentPropertyViewModel>()
            {
                new ComponentPropertyViewModel() { Name = "title", DataType = "string", Description = Resources.Components.PageHeading_Properties_Title },
                new ComponentPropertyViewModel() { Name = "description", DataType = "string", Description = Resources.Components.PageHeading_Properties_Description },
                new ComponentPropertyViewModel() { Name = "size", DataType = "PageHeadingSize", DefaultValue = "PageHeadingSize.Default", Description = Resources.Components.PageHeading_Properties_Size },
                new ComponentPropertyViewModel() { Name = "src", DataType = "string", Description = Resources.Components.PageHeading_Properties_Src },
                new ComponentPropertyViewModel() { Name = "text-emphasis", DataType = "bool", DefaultValue = "false", Description = Resources.Components.PageHeading_Properties_TextEmphasis }
            };
            vm.SampleCodeSections = new List<ComponentSampleCodeSectionViewModel>()
            {
                new ComponentSampleCodeSectionViewModel() { Id = Resources.Components.PageHeading_Basic_Anchor, PartialViewName = "PageHeading/_Basic", Title = Resources.Components.PageHeading_Basic_Title }
            };
            vm.Tag = "<fdcp-page-heading>";

            return vm;
        }
        private static ComponentViewModel BuildSearchableSelectComponentViewModel()
        {
            var vm = new ComponentViewModel();

            vm.Name = Resources.Components.SearchableSelect_Name;
            vm.Overview = Resources.Components.SearchableSelect_Overview;
            vm.Properties = new List<ComponentPropertyViewModel>()
            {
                new ComponentPropertyViewModel() { Name = "for", DataType = "ModelExpression", Description = Resources.Components.SearchableSelect_Properties_For },
                new ComponentPropertyViewModel() { Name = "name", DataType = "string", Description = Resources.Components.SearchableSelect_Properties_Name },
                new ComponentPropertyViewModel() { Name = "items", DataType = "IEnumerable<SelectListItem>", Description = Resources.Components.SearchableSelect_Properties_Items },
                new ComponentPropertyViewModel() { Name = "label", DataType = "string", Description = Resources.Components.SearchableSelect_Properties_Label },
                new ComponentPropertyViewModel() { Name = "default-value", DataType = "string", DefaultValue = "Select option", Description = Resources.Components.SearchableSelect_Properties_DefaultValue },
                new ComponentPropertyViewModel() { Name = "search-placeholder", DataType = "string", DefaultValue = "Search", Description = Resources.Components.SearchableSelect_Properties_SearchPlaceholder },
                new ComponentPropertyViewModel() { Name = "search-label", DataType = "string", DefaultValue = "Search options", Description = Resources.Components.SearchableSelect_Properties_SearchLabel },
                new ComponentPropertyViewModel() { Name = "no-results-text", DataType = "string", DefaultValue = "No results found", Description = Resources.Components.SearchableSelect_Properties_NoResultsText },
                new ComponentPropertyViewModel() { Name = "selection-mode", DataType = "FDCPSearchableSelectSelectionMode", DefaultValue = "Single", Description = Resources.Components.SearchableSelect_Properties_SelectionMode },
                new ComponentPropertyViewModel() { Name = "hint", DataType = "string", Description = Resources.Components.SearchableSelect_Properties_Hint },
                new ComponentPropertyViewModel() { Name = "required", DataType = "bool", Description = Resources.Components.SearchableSelect_Properties_Required }
            };
            vm.SampleCodeSections = new List<ComponentSampleCodeSectionViewModel>()
            {
                new ComponentSampleCodeSectionViewModel() { Id = Resources.Components.SearchableSelect_Basic_Anchor, PartialViewName = "SearchableSelect/_Basic", Title = Resources.Components.SearchableSelect_Basic_Title },
                new ComponentSampleCodeSectionViewModel() { Description = Resources.Components.SearchableSelect_WithSelectedOptions_Text, Id = Resources.Components.SearchableSelect_WithSelectedOptions_Anchor, PartialViewName = "SearchableSelect/_WithSelectedOptions", Title = Resources.Components.SearchableSelect_WithSelectedOptions_Title }
            };
            vm.Tag = "<fdcp-searchable-select>";

            return vm;
        }
        private static ComponentViewModel BuildStepperComponentViewModel()
        {
            var vm = new ComponentViewModel();

            vm.Name = Resources.Components.Stepper_Name;

            vm.AccessibilityDo = new List<string>()
            {
                Resources.Components.Stepper_Accessibility_Do_1,
                Resources.Components.Stepper_Accessibility_Do_2,
                Resources.Components.Stepper_Accessibility_Do_3
            };
            vm.Notes = new List<string>()
            {
                Resources.Components.Stepper_Notes_1,
                Resources.Components.Stepper_Notes_2,
                Resources.Components.Stepper_Notes_3,
                Resources.Components.Stepper_Notes_4
            };
            vm.Overview = Resources.Components.Stepper_Overview;
            vm.Properties = new List<ComponentPropertyViewModel>()
            {
                new ComponentPropertyViewModel() { Name = "current-step", DataType = "int", DefaultValue = "1", Description = Resources.Components.Stepper_Properties_CurrentStep },
                new ComponentPropertyViewModel() { Name = "steps", DataType = "IEnumerable<StepperStep>", Description = Resources.Components.Stepper_Properties_Steps },
                new ComponentPropertyViewModel() { Name = "StepperStep.StepNumber", DataType = "int", Description = Resources.Components.Stepper_Properties_StepperStep_StepNumber },
                new ComponentPropertyViewModel() { Name = "StepperStep.Label", DataType = "string", Description = Resources.Components.Stepper_Properties_StepperStep_Label },
                new ComponentPropertyViewModel() { Name = "StepperStep.DisplayMode", DataType = "string", DefaultValue = "StepperStepDisplayMode.Number", Description = Resources.Components.Stepper_Properties_StepperStep_DisplayMode },
                new ComponentPropertyViewModel() { Name = "StepperStep.IsHidden", DataType = "bool", Description = Resources.Components.Stepper_Properties_StepperStep_IsHidden },
                new ComponentPropertyViewModel() { Name = "StepperStep.IsLink", DataType = "bool", Description = Resources.Components.Stepper_Properties_StepperStep_IsLink },
                new ComponentPropertyViewModel() { Name = "StepperStep.LinkUrl", DataType = "string", Description = Resources.Components.Stepper_Properties_StepperStep_LinkUrl },
                new ComponentPropertyViewModel() { Name = "StepperStep.CompletedIconHtml", DataType = "string", Description = Resources.Components.Stepper_Properties_StepperStep_CompletedIconHtml },
                new ComponentPropertyViewModel() { Name = "StepperStep.InProgressIconHtml", DataType = "string", Description = Resources.Components.Stepper_Properties_StepperStep_InProgressIconHtml },
                new ComponentPropertyViewModel() { Name = "StepperStep.NotStartedIconHtml", DataType = "string", Description = Resources.Components.Stepper_Properties_StepperStep_NotStartedIconHtml },
                new ComponentPropertyViewModel() { Name = "StepperStep.StatusBadgeLabel", DataType = "string", Description = Resources.Components.Stepper_Properties_StepperStep_StatusBadgeLabel },
                new ComponentPropertyViewModel() { Name = "StepperStep.StatusBadgeStyle", DataType = "BadgeStyle", DefaultValue = "BadgeStyle.primary", Description = Resources.Components.Stepper_Properties_StepperStep_StatusBadgeStyle },
                new ComponentPropertyViewModel() { Name = "StepperStep.StatusBadgeStyleInverted", DataType = "bool", Description = Resources.Components.Stepper_Properties_StepperStep_StatusBadgeStyleInverted }
            };
            vm.SampleCodeSections = new List<ComponentSampleCodeSectionViewModel>()
            {
                new ComponentSampleCodeSectionViewModel() { Description = Resources.Components.Stepper_Basic_Text, Id = Resources.Components.Stepper_Basic_Anchor, PartialViewName = "Stepper/_Basic", Title = Resources.Components.Stepper_Basic_Title },
                new ComponentSampleCodeSectionViewModel() { Description = Resources.Components.Stepper_WithIcons_Text, Id = Resources.Components.Stepper_WithIcons_Anchor, PartialViewName = "Stepper/_WithIcons", Title = Resources.Components.Stepper_WithIcons_Title },
                new ComponentSampleCodeSectionViewModel() { Description = Resources.Components.Stepper_WithLinks_Text, Id = Resources.Components.Stepper_WithLinks_Anchor, PartialViewName = "Stepper/_WithLinks", Title = Resources.Components.Stepper_WithLinks_Title },
                new ComponentSampleCodeSectionViewModel() { Description = Resources.Components.Stepper_WithStatus_Text, Id = Resources.Components.Stepper_WithStatus_Anchor, PartialViewName = "Stepper/_WithStatus", Title = Resources.Components.Stepper_WithStatus_Title }
            };
            vm.Tag = "<fdcp-stepper>";

            return vm;
        }
        private static ComponentViewModel BuildTableComponentViewModel()
        {
            var vm = new ComponentViewModel();

            vm.Name = Resources.Components.Table_Name;
            vm.Tag = "<fdcp-table>";
            vm.Overview = Resources.Components.Table_Overview;

            vm.SampleCodeSections = new List<ComponentSampleCodeSectionViewModel>()
            {
                new ComponentSampleCodeSectionViewModel()
                {
                    Title = Resources.Components.Table_BasicUsage_Title,
                    Id = Resources.Components.Table_BasicUsage_Anchor,
                    Description = Resources.Components.Table_BasicUsage_Text,
                    PartialViewName = "Table/_BasicUsage"
                },
                new ComponentSampleCodeSectionViewModel()
                {
                    Title = Resources.Components.Table_WithSlots_Title,
                    Id = Resources.Components.Table_WithSlots_Anchor,
                    Description = Resources.Components.Table_WithSlots_Text,
                    PartialViewName = "Table/_WithSlots"
                },
                new ComponentSampleCodeSectionViewModel()
                {
                    Title = Resources.Components.Properties,
                    Id = Resources.Components.Properties_Anchor,
                    PartialViewName = "Table/_Properties"
                }
            };

            vm.SideNavigation = new SideNavigationViewModel()
            {
                Items = new List<NavItem>()
                {
                    new NavLink() { Href = Resources.Components.Overview_Anchor, Label = Resources.Components.Overview },
                    new NavGroup() { Label = Resources.Components.Table_BasicUsage_Title, Items = new List<NavItem>()
                        {
                            new NavLink() { Href = Resources.Components.Table_WithAnnotations_Anchor, Label = Resources.Components.Table_WithAnnotations_Title },
                            new NavLink() { Href = Resources.Components.Table_WithColumns_Anchor, Label = Resources.Components.Table_WithColumns_Title }
                        }
                    },
                    new NavGroup() { Label = Resources.Components.Table_WithSlots_Title, Items = new List<NavItem>()
                        {
                            new NavLink() { Href = Resources.Components.Table_WithEmail_Anchor, Label = Resources.Components.Table_WithEmail_Title },
                            new NavLink() { Href = Resources.Components.Table_WithLink_Anchor, Label = Resources.Components.Table_WithLink_Title },
                            new NavLink() { Href = Resources.Components.Table_WithButton_Anchor, Label = Resources.Components.Table_WithButton_Title },
                            new NavLink() { Href = Resources.Components.Table_WithButtonLink_Anchor, Label = Resources.Components.Table_WithButtonLink_Title }
                        }
                    },
                    new NavLink() { Href = Resources.Components.Properties_Anchor, Label = Resources.Components.Properties }
                }
            };

            return vm;
        }
        private FormDefinition GenerateSampleFormDefinition()
        {
            var form = new FormDefinition
            {
                Id = "demo-form",
                Title = "Dynamic Form Demo",
                Action = Url.Action("FormBuilder", "Components") ?? "",
                Method = "post",
                SubmitButtonText = "Submit Form",
                Sections = new List<FormSection>
                {
                    new FormSection
                    {
                        Title = "Personal Information",
                        Hint = "Please provide your basic information",
                        Questions = new List<FormQuestion>
                        {
                            new FormQuestion
                            {
                                Id = "fullName",
                                Label = "Full Name",
                                Type = QuestionType.Text,
                                IsRequired = true,
                                Hint = "Enter your full legal name"
                            },
                            new FormQuestion
                            {
                                Id = "email",
                                Label = "Email Address",
                                Type = QuestionType.Email,
                                IsRequired = true,
                                Hint = "We'll use this for communication"
                            }
                        }
                    },
                    new FormSection
                    {
                        Title = "Location Information",
                        Hint = "Tell us where you're located",
                        Questions = new List<FormQuestion>
                        {
                            // Country selection with cascading dependencies
                            new FormQuestion
                            {
                                Id = "country",
                                Label = "Country of Residence",
                                Type = QuestionType.Dropdown,
                                IsRequired = true,
                                Options = new List<QuestionOption>
                                {
                                    new() { Id = "ca", Value = "CA", Label = "Canada" },
                                    new() { Id = "us", Value = "US", Label = "United States" },
                                    new() { Id = "other", Value = "OTHER", Label = "Other" }
                                }
                            },
                            // Province field - shows when Canada is selected
                            new FormQuestion
                            {
                                Id = "province",
                                Label = "Province",
                                Type = QuestionType.Dropdown,
                                Dependencies = new List<QuestionDependency>
                                {
                                    new()
                                    {
                                        SourceQuestionId = "country",
                                        TargetQuestionId = "province",
                                        Action = DependencyAction.Show,
                                        TriggerValue = "CA"
                                    },
                                    new()
                                    {
                                        SourceQuestionId = "country",
                                        TargetQuestionId = "province",
                                        Action = DependencyAction.Require,
                                        TriggerValue = "CA"
                                    }
                                },
                                Options = new List<QuestionOption>
                                {
                                    new() { Id = "on", Value = "ON", Label = "Ontario" },
                                    new() { Id = "qc", Value = "QC", Label = "Quebec" },
                                    new() { Id = "bc", Value = "BC", Label = "British Columbia" }
                                }
                            },
                            // State field - shows when US is selected
                            new FormQuestion
                            {
                                Id = "state",
                                Label = "State",
                                Type = QuestionType.Dropdown,
                                Dependencies = new List<QuestionDependency>
                                {
                                    new()
                                    {
                                        SourceQuestionId = "country",
                                        TargetQuestionId = "state",
                                        Action = DependencyAction.Show,
                                        TriggerValue = "US"
                                    },
                                    new()
                                    {
                                        SourceQuestionId = "country",
                                        TargetQuestionId = "state",
                                        Action = DependencyAction.Require,
                                        TriggerValue = "US"
                                    }
                                },
                                Options = new List<QuestionOption>
                                {
                                    new() { Id = "ny", Value = "NY", Label = "New York" },
                                    new() { Id = "ca", Value = "CA", Label = "California" },
                                    new() { Id = "tx", Value = "TX", Label = "Texas" }
                                }
                            },
                            // Other Country field - shows when Other is selected
                            new FormQuestion
                            {
                                Id = "otherCountry",
                                Label = "Specify Country",
                                Type = QuestionType.Text,
                                Dependencies = new List<QuestionDependency>
                                {
                                    new()
                                    {
                                        SourceQuestionId = "country",
                                        TargetQuestionId = "otherCountry",
                                        Action = DependencyAction.Show,
                                        TriggerValue = "OTHER"
                                    },
                                    new()
                                    {
                                        SourceQuestionId = "country",
                                        TargetQuestionId = "otherCountry",
                                        Action = DependencyAction.Require,
                                        TriggerValue = "OTHER"
                                    }
                                }
                            }
                        }
                    },
                    new FormSection
                    {
                        Title = "Service Selection",
                        Hint = "Choose your service preferences",
                        Questions = new List<FormQuestion>
                        {
                            // Service Type with multiple dependent fields
                            new FormQuestion
                            {
                                Id = "serviceType",
                                Label = "Service Type",
                                Type = QuestionType.Radio,
                                IsRequired = true,
                                Options = new List<QuestionOption>
                                {
                                    new() { Id = "basic", Value = "BASIC", Label = "Basic Service" },
                                    new() { Id = "premium", Value = "PREMIUM", Label = "Premium Service" },
                                    new() { Id = "custom", Value = "CUSTOM", Label = "Custom Service" }
                                }
                            },
                            // Premium features - shown and required for premium service
                            new FormQuestion
                            {
                                Id = "premiumFeatures",
                                Label = "Premium Features",
                                Type = QuestionType.Checkbox,
                                Hint = "Select the premium features you want",
                                Dependencies = new List<QuestionDependency>
                                {
                                    new()
                                    {
                                        SourceQuestionId = "serviceType",
                                        TargetQuestionId = "premiumFeatures",
                                        Action = DependencyAction.Show,
                                        TriggerValue = "PREMIUM"
                                    },
                                    new()
                                    {
                                        SourceQuestionId = "serviceType",
                                        TargetQuestionId = "premiumFeatures",
                                        Action = DependencyAction.Require,
                                        TriggerValue = "PREMIUM"
                                    }
                                },
                                Options = new List<QuestionOption>
                                {
                                    new() { Id = "feature1", Value = "24_7_SUPPORT", Label = "24/7 Support" },
                                    new() { Id = "feature2", Value = "PRIORITY", Label = "Priority Service" },
                                    new() { Id = "feature3", Value = "ADVANCED", Label = "Advanced Features" }
                                }
                            },
                            // Custom requirements - shown and enabled for custom service
                            new FormQuestion
                            {
                                Id = "customRequirements",
                                Label = "Custom Requirements",
                                Type = QuestionType.TextArea,
                                Hint = "Describe your custom service needs",
                                Dependencies = new List<QuestionDependency>
                                {
                                    new()
                                    {
                                        SourceQuestionId = "serviceType",
                                        TargetQuestionId = "customRequirements",
                                        Action = DependencyAction.Show,
                                        TriggerValue = "CUSTOM"
                                    },
                                    new()
                                    {
                                        SourceQuestionId = "serviceType",
                                        TargetQuestionId = "customRequirements",
                                        Action = DependencyAction.Require,
                                        TriggerValue = "CUSTOM"
                                    }
                                }
                            },
                            // Budget range - disabled for basic service
                            new FormQuestion
                            {
                                Id = "budgetRange",
                                Label = "Budget Range",
                                Type = QuestionType.Dropdown,
                                Dependencies = new List<QuestionDependency>
                                {
                                    new()
                                    {
                                        SourceQuestionId = "serviceType",
                                        TargetQuestionId = "budgetRange",
                                        Action = DependencyAction.Disable,
                                        TriggerValue = "BASIC"
                                    }
                                },
                                Options = new List<QuestionOption>
                                {
                                    new() { Id = "budget1", Value = "UNDER_1000", Label = "Under $1,000" },
                                    new() { Id = "budget2", Value = "1000_5000", Label = "$1,000 - $5,000" },
                                    new() { Id = "budget3", Value = "OVER_5000", Label = "Over $5,000" }
                                }
                            }
                        }
                    },
                    new FormSection
                    {
                        Title = "Additional Information",
                        Questions = new List<FormQuestion>
                        {
                            // Contact preference with dependent phone field
                            new FormQuestion
                            {
                                Id = "contactPreference",
                                Label = "Preferred Contact Method",
                                Type = QuestionType.Radio,
                                IsRequired = true,
                                Options = new List<QuestionOption>
                                {
                                    new() { Id = "email", Value = "EMAIL", Label = "Email" },
                                    new() { Id = "phone", Value = "PHONE", Label = "Phone" }
                                }
                            },
                            // Phone number - required when phone is selected
                            new FormQuestion
                            {
                                Id = "phoneNumber",
                                Label = "Phone Number",
                                Type = QuestionType.Text,
                                Dependencies = new List<QuestionDependency>
                                {
                                    new()
                                    {
                                        SourceQuestionId = "contactPreference",
                                        TargetQuestionId = "phoneNumber",
                                        Action = DependencyAction.Show,
                                        TriggerValue = "PHONE"
                                    },
                                    new()
                                    {
                                        SourceQuestionId = "contactPreference",
                                        TargetQuestionId = "phoneNumber",
                                        Action = DependencyAction.Require,
                                        TriggerValue = "PHONE"
                                    }
                                }
                            },
                            new FormQuestion
                            {
                                Id = "projectSummary",
                                Label = "Project Summary",
                                Type = QuestionType.RichText,
                                Hint = "Provide a summary (you can format text, add lists, etc.)",
                                IsRequired = true,
                                Placeholder = "Describe your project goals, milestones and outcomes.",
                                Height = "260px",
                                RichTextToolbar = FDCPRichTextToolbar.Standard,
                                Templates = new Dictionary<string, string>
                                {
                                    {
                                        "Accordion",
                                        "<details class='gcds-details'><summary>Accordion title</summary><p>Accordion body content.</p></details>"
                                    },
                                    {
                                        "Callout",
                                        "<div class='fdcp-callout'><strong>Callout title</strong><p>Important supporting details go here.</p></div>"
                                    }
                                }
                            },
                            // Terms acceptance
                            new FormQuestion
                            {
                                Id = "termsAccepted",
                                Label = "Terms and Conditions",
                                Type = QuestionType.Checkbox,
                                IsRequired = true,
                                Options = new List<QuestionOption>
                                {
                                    new() { Id = "terms", Value = "true", Label = "I accept the terms and conditions" }
                                }
                            }
                        }
                    }
                }
            };
            return form;
        }
        #endregion ViewModel Building
    }
}