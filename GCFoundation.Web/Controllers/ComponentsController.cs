using GCFoundation.Components.Controllers;
using GCFoundation.Components.Enums;
using GCFoundation.Web.Models;
using GCFoundation.Web.Models.Components;
using GCFoundation.Web.Resources;
using GCFoundation.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GCFoundation.Web.Controllers
{
    /// <summary>
    /// Controller that handles requests related to reusable UI components.
    /// </summary>
    [Route("components")]
    public class ComponentsController(ILogger<ComponentsController> logger, ITagHelperCatalogService tagHelperCatalogService) : GCFoundationBaseController(logger)
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

            var vm = BuildIndexComponentsPageViewModel();
            return View(vm);
        }

        /// <summary>
        /// Displays the Accordion component demo page.
        /// </summary>
        /// <returns>The Accordion component view.</returns>
        [HttpGet("accordion")]
        public IActionResult Accordion()
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_Accordion_Title}");

            var vm = BuildAccordionComponentViewModel();
            return View("Component", vm);
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

        /// <summary>Displays the code component guide.</summary>
        /// <returns>The standard component documentation view.</returns>
        [HttpGet("code")]
        public IActionResult Code()
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_Code_Title}");
            return View("Component", BuildCodeComponentViewModel());
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
        /// Displays the Likert Scale component documentation page.
        /// </summary>
        /// <returns>The Likert Scale component view.</returns>
        [HttpGet("likert-scale")]
        public IActionResult LikertScale()
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_LikertScale_Title}");

            var vm = BuildLikertScaleComponentViewModel();
            return View("LikertScale/Index", vm);
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
            return View("Component", vm);
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
        /// Displays the Tabs component demo page.
        /// </summary>
        /// <returns>
        /// The Tabs component view.
        /// </returns>
        [HttpGet("tabs")]
        public IActionResult Tabs()
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_Tabs_Title}");

            var vm = BuildTabsComponentViewModel();
            return View("Component", vm);
        }

        /// <summary>
        /// Returns sample server-loaded content for the Tabs component demo.
        /// </summary>
        /// <returns>A partial HTML fragment for a tab panel.</returns>
        [HttpGet("tabs/lazy-content")]
        public IActionResult TabsLazyContent()
        {
            var content = System.Net.WebUtility.HtmlEncode(Resources.Components.Tabs_LazyContent);
            return Content($"<gcds-text>{content}</gcds-text>", "text/html");
        }

        /// <summary>
        /// Displays the Account Partial component demo page.
        /// </summary>
        /// <returns>
        /// The Account Partial component view.
        /// </returns>
        [HttpGet("account")]
        public IActionResult Account()
        {
            SetPageTitle($"{Menu.Menu_Components} : {Resources.Components.Index_AccountPartial_Title}");
            ViewData["AccountPartialViewName"] = "_ExampleAccount";
            return View();
        }


        #region ViewModel Building
        private static ComponentViewModel BuildAccordionComponentViewModel()
        {
            var vm = new ComponentViewModel
            {
                Name = Resources.Components.Accordion_Name,
                Tag = "<fdcp-accordion>",
                Overview = Resources.Components.Accordion_Overview,
                SampleCodeSections = new List<ComponentSampleCodeSectionViewModel>()
                {
                    new()
                    {
                        Title = Resources.Components.Accordion_BasicUsage_Title,
                        Id = Resources.Components.Accordion_BasicUsage_Anchor,
                        Description = Resources.Components.Accordion_BasicUsage_Text,
                        PartialViewName = "Accordion/_Basic"
                    },
                    new()
                    {
                        Title = Resources.Components.Properties,
                        Id = Resources.Components.Properties_Anchor,
                        PartialViewName = "Accordion/_Properties"
                    }
                },
                SideNavigation = new SideNavigationViewModel
                {
                    Items = new List<NavItem>()
                    {
                        new NavLink() { Href = Resources.Components.Overview_Anchor, Label = Resources.Components.Overview },
                        new NavGroup()
                        {
                            Label = Resources.Components.Accordion_BasicUsage_Title,
                            Items = new List<NavItem>()
                            {
                                new NavLink() { Href = Resources.Components.Accordion_BasicAccordion_Anchor, Label = Resources.Components.Accordion_BasicAccordion_Title },
                                new NavLink() { Href = Resources.Components.Accordion_AlwaysOpen_Anchor, Label = Resources.Components.Accordion_AlwaysOpen_Title },
                                new NavLink() { Href = Resources.Components.Accordion_ButtonsPosition_Anchor, Label = Resources.Components.Accordion_ButtonsPosition_Title }
                            }
                        },
                        new NavLink() { Href = Resources.Components.Properties_Anchor, Label = Resources.Components.Properties }
                    }
                }
            };

            return vm;
        }
        private static ComponentViewModel BuildBadgeComponentViewModel()
        {
            var vm = new ComponentViewModel();

            vm.Name = Resources.Components.Badge_Name;
            vm.Notes = new List<string>()
            {
                Resources.Components.Badge_Notes_1,
                Resources.Components.Badge_Notes_2,
                Resources.Components.Badge_Notes_3,
                Resources.Components.Badge_Notes_4
            };
            vm.Overview = Resources.Components.Badge_Overview;
            vm.AlsoCalled = Resources.Components.Badge_AlsoCalled;
            vm.WhenToUse = new List<string>()
            {
                Resources.Components.Badge_WhenToUse_1,
                Resources.Components.Badge_WhenToUse_2,
                Resources.Components.Badge_WhenToUse_3,
                Resources.Components.Badge_WhenToUse_4,
                Resources.Components.Badge_WhenToUse_5,
                Resources.Components.Badge_WhenToUse_6
            };
            vm.WhenToUseExamples = new List<string>()
            {
                Resources.Components.Badge_WhenToUse_Example_1,
                Resources.Components.Badge_WhenToUse_Example_2,
                Resources.Components.Badge_WhenToUse_Example_3,
                Resources.Components.Badge_WhenToUse_Example_4,
                Resources.Components.Badge_WhenToUse_Example_5,
                Resources.Components.Badge_WhenToUse_Example_6,
                Resources.Components.Badge_WhenToUse_Example_7,
                Resources.Components.Badge_WhenToUse_Example_8
            };
            vm.WhenNotToUse = new List<string>()
            {
                Resources.Components.Badge_WhenNotToUse_1,
                Resources.Components.Badge_WhenNotToUse_2,
                Resources.Components.Badge_WhenNotToUse_3,
                Resources.Components.Badge_WhenNotToUse_4,
                Resources.Components.Badge_WhenNotToUse_5,
                Resources.Components.Badge_WhenNotToUse_6
            };
            vm.WhenNotToUseNote = Resources.Components.Badge_WhenNotToUse_Note;
            vm.GuidanceSections = new List<ComponentSampleCodeSectionViewModel>()
            {
                new ComponentSampleCodeSectionViewModel() { Id = Resources.Components.Guidance_Accessibility_Anchor, PartialViewName = "Badge/_Accessibility", Title = Resources.Components.Guidance_Accessibility }
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
                new ComponentSampleCodeSectionViewModel() { Description = Resources.Components.Card_WithSlots_Text, Id = Resources.Components.Card_WithSlots_Anchor, PartialViewName = "Card/_WithSlots", Title = Resources.Components.Card_WithSlots_Title },
                new ComponentSampleCodeSectionViewModel() { Id = Resources.Components.Card_CompositionExamples_Anchor, PartialViewName = "Card/_CompositionExamples", Title = Resources.Components.Card_CompositionExamples_Title }
            };
            vm.Tag = "<fdcp-card>";
            vm.SideNavigation = new SideNavigationViewModel()
            {
                Items = new List<NavItem>()
                {
                    new NavLink() { Href = Resources.Components.Overview_Anchor, Label = Resources.Components.Overview },
                    new NavLink() { Href = Resources.Components.Card_Basic_Anchor, Label = Resources.Components.Card_Basic_Title },
                    new NavLink() { Href = Resources.Components.Card_Horizontal_Anchor, Label = Resources.Components.Card_Horizontal_Title },
                    new NavLink() { Href = Resources.Components.Card_WithImages_Anchor, Label = Resources.Components.Card_WithImages_Title },
                    new NavLink() { Href = Resources.Components.Card_WithSlots_Anchor, Label = Resources.Components.Card_WithSlots_Title },
                    new NavGroup() { Label = Resources.Components.Card_CompositionExamples_Title, Items = new List<NavItem>()
                        {
                            new NavLink() { Href = Resources.Components.Card_ListItem_Anchor, Label = Resources.Components.Card_ListItem_Title }
                        }
                    },
                    new NavLink() { Href = Resources.Components.Properties_Anchor, Label = Resources.Components.Properties },
                    new NavLink() { Href = Resources.Components.Notes_Anchor, Label = Resources.Components.Notes }
                }
            };

            return vm;
        }
        private static ComponentViewModel BuildCodeComponentViewModel()
        {
            return new ComponentViewModel
            {
                Name = Resources.Components.Code_Name,
                Tag = "<fdcp-code>",
                Overview = Resources.Components.Code_Overview,
                Purpose = Resources.Components.Code_Purpose,
                WhenToUse = [Resources.Components.Code_WhenUse1, Resources.Components.Code_WhenUse2],
                WhenNotToUse = [Resources.Components.Code_WhenNot1, Resources.Components.Code_WhenNot2],
                AccessibilityDo = [Resources.Components.Code_Accessibility1, Resources.Components.Code_Accessibility2, Resources.Components.Code_Accessibility3, Resources.Components.Code_Accessibility4],
                UxBestPractices = [Resources.Components.Code_Ux1, Resources.Components.Code_Ux2],
                Notes = [Resources.Components.Code_NoteLanguages, Resources.Components.Code_NoteEncoding, Resources.Components.Code_NoteProgressive, Resources.Components.Code_NoteLimit, Resources.Components.Code_NoteTheme, Resources.Components.Code_NoteDynamic],
                Properties =
                [
                    new() { Name = "code", DataType = "string", Description = Resources.Components.Code_PropCode },
                    new() { Name = "copy-enabled", DataType = "bool", DefaultValue = "true", Description = Resources.Components.Code_PropCopyEnabled },
                    new() { Name = "id", DataType = "string", Description = Resources.Components.Code_PropId },
                    new() { Name = "code-language", DataType = "string", DefaultValue = "plaintext", Description = Resources.Components.Code_PropCodeLanguage },
                    new() { Name = "code-language-label", DataType = "string", Description = Resources.Components.Code_PropCodeLanguageLabel },
                    new() { Name = "title", DataType = "string", Description = Resources.Components.Code_PropTitle },
                    new() { Name = "wrap", DataType = "bool", DefaultValue = "false", Description = Resources.Components.Code_PropWrap },
                    new() { Name = "examples", DataType = "IEnumerable<CodeSample>", Description = Resources.Components.Code_PropExamples },
                    new() { Name = "selected-code-language", DataType = "string", Description = Resources.Components.Code_PropSelectedCodeLanguage },
                    new() { Name = "sync-group", DataType = "string", Description = Resources.Components.Code_PropSyncGroup },
                    new() { Name = "theme", DataType = "ColourTheme", DefaultValue = "light", Description = Resources.Components.Code_PropTheme }
                ],
                SampleCodeSections =
                [
                    new() { Id = "code-block", Title = Resources.Components.Code_BlockTitle, Description = Resources.Components.Code_BlockText, PartialViewName = "Code/_Block" },
                    new() { Id = "code-example", Title = Resources.Components.Code_ExampleTitle, Description = Resources.Components.Code_ExampleText, PartialViewName = "Code/_Example" },
                    new() { Id = "code-dark", Title = Resources.Components.Code_DarkTitle, Description = Resources.Components.Code_DarkText, PartialViewName = "Code/_Dark" },
                    new() { Id = "code-fallback", Title = Resources.Components.Code_FallbackTitle, Description = Resources.Components.Code_FallbackText, PartialViewName = "Code/_Fallback" }
                ]
            };
        }
        private static FormTestViewModel BuildFormComponentViewModel(FormTestViewModel? vm = null)
        {
            if (vm == null)
                vm = new FormTestViewModel();

            vm.Name = Resources.Components.Form_Name;
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
        private static LikertScaleModel BuildLikertScaleComponentViewModel()
        {
            return new LikertScaleModel
            {
                Name = Resources.Components.Index_LikertScale_Title,
                Overview = Resources.Components.LikertScale_Overview,
                Tag = "<fdcp-likert-scale>",
                WhenToUse =
                [
                    Resources.Components.LikertScale_WhenToUse_MultipleQuestions,
                    Resources.Components.LikertScale_WhenToUse_QuestionCount
                ],
                Properties =
                [
                    new() { Name = "disabled", DataType = "bool", DefaultValue = "false", Description = Resources.Components.LikertScale_Properties_Disabled },
                    new() { Name = "for", DataType = "ModelExpression", Description = Resources.Components.LikertScale_Properties_For },
                    new() { Name = "hint", DataType = "string", Description = Resources.Components.LikertScale_Properties_Hint },
                    new() { Name = "id", DataType = "string", Description = Resources.Components.LikertScale_Properties_Id },
                    new() { Name = "index", DataType = "string", Description = Resources.Components.LikertScale_Properties_Index },
                    new() { Name = "items", DataType = "IEnumerable<SelectListItem>", Description = Resources.Components.LikertScale_Properties_Items },
                    new() { Name = "legend", DataType = "string", Description = Resources.Components.LikertScale_Properties_Legend },
                    new() { Name = "name", DataType = "string", Description = Resources.Components.LikertScale_Properties_Name },
                    new() { Name = "required", DataType = "bool", Description = Resources.Components.LikertScale_Properties_Required },
                    new() { Name = "value", DataType = "string", Description = Resources.Components.LikertScale_Properties_Value }
                ],
                SampleCodeSections =
                [
                    new ComponentSampleCodeSectionViewModel
                    {
                        Id = Resources.Components.LikertScale_ModelBinding_Anchor,
                        PartialViewName = "LikertScale/_Basic",
                        Title = Resources.Components.LikertScale_ModelBinding_Title,
                        Description = Resources.Components.LikertScale_ModelBinding_Description
                    },
                    new ComponentSampleCodeSectionViewModel
                    {
                        Id = Resources.Components.LikertScale_ManualEntry_Anchor,
                        PartialViewName = "LikertScale/_ManualEntry",
                        Title = Resources.Components.LikertScale_ManualEntry_Title,
                        Description = Resources.Components.LikertScale_ManualEntry_Description
                    }
                ]
            };
        }
        private ComponentsIndexPageViewModel BuildIndexComponentsPageViewModel()
        {
            return new ComponentsIndexPageViewModel()
            {
                FeaturedComponents = new List<ComponentIndexViewModel>()
                {
                    new () { Name = Resources.Components.Index_Accordion_Title, ShortDescription = Resources.Components.Index_Accordion_Description, Href = Url.Action("Accordion", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-accordion.svg"), IsNew = true },
                    new () { Name = Resources.Components.Index_Badge_Title, Description = Resources.Components.Index_Badge_Description, Href = Url.Action("Badge", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-badge.svg") },
                    new () { Name = Resources.Components.Index_Card_Title, Description = Resources.Components.Index_Card_Description, Href = Url.Action("Card", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-card.svg") },
                    new () { Name = Resources.Components.Index_Code_Title, Description = Resources.Components.Index_Code_Description, Href = Url.Action("Code", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-code.svg"), IsNew = true },
                    new () { Name = Resources.Components.Index_Form_Title, ShortDescription = Resources.Components.Index_Form_Description, Href = Url.Action("Form", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-form.svg") },
                    new () { Name = Resources.Components.Index_LikertScale_Title, ShortDescription = Resources.Components.Index_LikertScale_Description, Href = Url.Action("LikertScale", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-radio.svg"), IsNew = true },
                    new () { Name = Resources.Components.Index_Modal_Title, ShortDescription = Resources.Components.Index_Modal_Description, Href = Url.Action("Modal", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-modal.svg") },
                    new () { Name = Resources.Components.Index_PageHeading_Title, ShortDescription = Resources.Components.Index_PageHeading_Description, Href = Url.Action("PageHeading", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-page-heading.svg") },
                    new () { Name = Resources.Components.Index_SearchableSelect_Title, ShortDescription = Resources.Components.Index_SearchableSelect_Description, Href = Url.Action("SearchableSelect", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-select.svg"), IsNew = true },
                    new () { Name = Resources.Components.Index_Stepper_Title, ShortDescription = Resources.Components.Index_Stepper_Description, Href = Url.Action("Stepper", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-stepper-fdcp.svg") },
                    new () { Name = Resources.Components.Index_Tabs_Title, ShortDescription = Resources.Components.Index_Tabs_Description, Href = Url.Action("Tabs", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-tabs.svg"), IsNew = true },
                    new () { Name = Resources.Components.Index_Table_Title, ShortDescription = Resources.Components.Index_Table_Description, Href = Url.Action("Table", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-table.svg"), IsNew = true },
                    new () { Name = Resources.Components.Index_AccountPartial_Title, ShortDescription = Resources.Components.Index_AccountPartial_Description, Href = Url.Action("Account", "Components") ?? string.Empty, ImgSrc = Url.Content("~/images/preview-account-partial.svg") }
                },
                TagHelperGroups = tagHelperCatalogService.BuildTagHelperGroups().ToList()
            };
        }
        private static ComponentViewModel BuildModalComponentViewModel()
        {
            var vm = new ComponentViewModel();

            vm.Name = Resources.Components.Modal_Name;
            vm.WhenToUse = new List<string>()
            {
                Resources.Components.Modal_WhenToUse_1,
                Resources.Components.Modal_WhenToUse_2,
                Resources.Components.Modal_WhenToUse_3,
                Resources.Components.Modal_WhenToUse_4
            };
            vm.WhenNotToUse = new List<string>()
            {
                Resources.Components.Modal_WhenNotToUse_1,
                Resources.Components.Modal_WhenNotToUse_2,
                Resources.Components.Modal_WhenNotToUse_3,
                Resources.Components.Modal_WhenNotToUse_4
            };
            vm.AccessibilityDo = new List<string>()
            {
                Resources.Components.Modal_Accessibility_Do_5,
                Resources.Components.Modal_Accessibility_Do_6,
                Resources.Components.Modal_Accessibility_Do_3,
                Resources.Components.Modal_Accessibility_Do_4,
                Resources.Components.Modal_Accessibility_Do_1,
                Resources.Components.Modal_Accessibility_Do_2
            };
            vm.Notes = new List<string>()
            {
                Resources.Components.Modal_Notes_6,
                Resources.Components.Modal_Notes_1,
                Resources.Components.Modal_Notes_2,
                Resources.Components.Modal_Notes_3,
                Resources.Components.Modal_Notes_4,
                Resources.Components.Modal_Notes_5
            };
            vm.Overview = Resources.Components.Modal_Overview;
            vm.Properties = new List<ComponentPropertyViewModel>()
            {
                new ComponentPropertyViewModel() { Name = "modal-id", DataType = "string", Description = Resources.Components.Modal_Properties_ModalId },
                new ComponentPropertyViewModel() { Name = "title", DataType = "string", Description = Resources.Components.Modal_Properties_Title },
                new ComponentPropertyViewModel() { Name = "scrollable", DataType = "bool", DefaultValue = "false", Description = Resources.Components.Modal_Properties_Scrollable },
                new ComponentPropertyViewModel() { Name = "static-backdrop", DataType = "bool", DefaultValue = "false", Description = Resources.Components.Modal_Properties_StaticBackdrop },
                new ComponentPropertyViewModel() { Name = "hide-close-button", DataType = "bool", DefaultValue = "false", Description = Resources.Components.Modal_Properties_HideCloseButton },
                new ComponentPropertyViewModel() { Name = "size", DataType = "ModalSize", DefaultValue = "ModalSize.regular", Description = Resources.Components.Modal_Properties_Size },
                new ComponentPropertyViewModel() { Name = "state", DataType = "ModalState", DefaultValue = "ModalState.regular", Description = Resources.Components.Modal_Properties_State },
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
            vm.SideNavigation = new SideNavigationViewModel()
            {
                Items = new List<NavItem>()
                {
                    new NavLink() { Href = Resources.Components.Overview_Anchor, Label = Resources.Components.Overview },
                    new NavLink() { Href = Resources.Components.Guidance_WhenToUse_Anchor, Label = Resources.Components.Guidance_WhenToUse },
                    new NavLink() { Href = Resources.Components.Guidance_WhenNotToUse_Anchor, Label = Resources.Components.Guidance_WhenNotToUse },
                    new NavLink() { Href = Resources.Components.Guidance_Accessibility_Anchor, Label = Resources.Components.Guidance_Accessibility },
                    new NavGroup()
                    {
                        Label = Resources.Components.Modal_Basic_Title,
                        Items = new List<NavItem>()
                        {
                            new NavLink() { Href = Resources.Components.Modal_BasicStructure_Anchor, Label = Resources.Components.Modal_BasicStructure_Title },
                            new NavLink() { Href = Resources.Components.Modal_ButtonBehaviour_Anchor, Label = Resources.Components.Modal_ButtonBehaviour_Title },
                            new NavLink() { Href = Resources.Components.Modal_Variants_Anchor, Label = Resources.Components.Modal_Variants_Title },
                            new NavLink() { Href = Resources.Components.Modal_Size_Anchor, Label = Resources.Components.Modal_Size_Title },
                            new NavLink() { Href = Resources.Components.Modal_AdditionalProperties_Anchor, Label = Resources.Components.Modal_AdditionalProperties_Title }
                        }
                    },
                    new NavLink() { Href = Resources.Components.Modal_Session_Anchor, Label = Resources.Components.Modal_Session_Title },
                    new NavLink() { Href = Resources.Components.Properties_Anchor, Label = Resources.Components.Properties },
                    new NavLink() { Href = Resources.Components.Notes_Anchor, Label = Resources.Components.Notes }
                }
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
                            new NavLink() { Href = Resources.Components.Table_DataBinding_Anchor, Label = Resources.Components.Table_DataBinding_Title },
                            new NavLink() { Href = Resources.Components.Table_WithEmail_Anchor, Label = Resources.Components.Table_WithEmail_Title },
                            new NavLink() { Href = Resources.Components.Table_WithLink_Anchor, Label = Resources.Components.Table_WithLink_Title },
                            new NavLink() { Href = Resources.Components.Table_WithButton_Anchor, Label = Resources.Components.Table_WithButton_Title },
                            new NavLink() { Href = Resources.Components.Table_WithButtonLink_Anchor, Label = Resources.Components.Table_WithButtonLink_Title },
                            new NavLink() { Href = Resources.Components.Table_WithCaptionSlot_Anchor, Label = Resources.Components.Table_WithCaptionSlot_Title }
                        }
                    },
                    new NavLink() { Href = Resources.Components.Properties_Anchor, Label = Resources.Components.Properties }
                }
            };

            return vm;
        }
        private static ComponentViewModel BuildTabsComponentViewModel()
        {
            var vm = new ComponentViewModel
            {
                Name = Resources.Components.Tabs_Name,
                Overview = Resources.Components.Tabs_Overview,
                Purpose = Resources.Components.Tabs_Purpose,
                Tag = "<fdcp-tabs>"
            };

            vm.WhenToUse = new List<string>
            {
                Resources.Components.Tabs_WhenToUse_1,
                Resources.Components.Tabs_WhenToUse_2
            };
            vm.WhenNotToUse = new List<string>
            {
                Resources.Components.Tabs_WhenNotToUse_1,
                Resources.Components.Tabs_WhenNotToUse_2
            };
            vm.AccessibilityDo = new List<string>
            {
                Resources.Components.Tabs_Accessibility_Do_1,
                Resources.Components.Tabs_Accessibility_Do_2,
                Resources.Components.Tabs_Accessibility_Do_3
            };
            vm.UxBestPractices = new List<string>
            {
                Resources.Components.Tabs_UxBestPractices_1,
                Resources.Components.Tabs_UxBestPractices_2
            };
            vm.Notes = new List<string>
            {
                Resources.Components.Tabs_Notes_1,
                Resources.Components.Tabs_Notes_2,
                Resources.Components.Tabs_Notes_3
            };
            vm.Properties = new List<ComponentPropertyViewModel>
            {
                new() { Name = "id", DataType = "string", Description = Resources.Components.Tabs_Properties_Id },
                new() { Name = "label", DataType = "string", DefaultValue = Resources.Components.Tabs_DefaultLabel, Description = Resources.Components.Tabs_Properties_Label },
                new() { Name = "selected-index", DataType = "int?", Description = Resources.Components.Tabs_Properties_SelectedIndex },
                new() { Name = "fdcp-tab title", DataType = "string", Description = Resources.Components.Tabs_Properties_TabTitle },
                new() { Name = "fdcp-tab id", DataType = "string", Description = Resources.Components.Tabs_Properties_TabId },
                new() { Name = "fdcp-tab active", DataType = "bool", DefaultValue = "false", Description = Resources.Components.Tabs_Properties_TabActive },
                new() { Name = "fdcp-tab load-url", DataType = "Uri", Description = Resources.Components.Tabs_Properties_TabLoadUrl }
            };
            vm.SampleCodeSections = new List<ComponentSampleCodeSectionViewModel>
            {
                new() { Description = Resources.Components.Tabs_Basic_Text, Id = Resources.Components.Tabs_Basic_Anchor, PartialViewName = "Tabs/_Basic", Title = Resources.Components.Tabs_Basic_Title }
            };

            return vm;
        }
        #endregion ViewModel Building
    }
}