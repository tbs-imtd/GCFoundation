using GCFoundation.Components.Attributes;
using GCFoundation.Components.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GCFoundation.Web.Models.Template
{
    /// <summary>
    /// View model for the Stepper template demo form.
    /// Each step owns a distinct set of fields; validation is applied per step in the controller.
    /// Every validated field has a unique, bilingual ErrorMessage for WCAG 3.3.1 / 3.3.3.
    /// </summary>
    public sealed class TemplateStepperFormViewModel : BaseViewModel
    {
        /// <summary>
        /// Field names belonging to each step. Used by the controller to scope ModelState validation
        /// and by the view to round-trip non-visible step values through hidden inputs.
        /// </summary>
        public static IReadOnlyDictionary<int, string[]> FieldsByStep { get; } = new Dictionary<int, string[]>
        {
            [1] = ["FirstName", "LastName", "Province", "ContactOptions"],
            [2] = ["Bio", "DateOfBirth", "SelectedCountry", "Gender"],
            [3] = ["Website", "Age"],
            [4] = [],
            [5] = ["AgreeToTerms"]
        };

        /// <summary>
        /// Gets or sets the current step in the demo. This is round-tripped through a hidden field
        /// so the server can re-render the same progress context after each post.
        /// </summary>
        public int CurrentStep { get; set; } = 1;

        /// <summary>
        /// Total number of steps in the demo Stepper. The controller uses this to clamp navigation
        /// and the component uses it to announce accurate progress to assistive technology.
        /// </summary>
        public int TotalSteps { get; } = 5;

        // ── Step 1: Intro ──────────────────────────────────────────────────

        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        [Required(
            ErrorMessageResourceName = "Stepper_Demo_FirstName_Required",
            ErrorMessageResourceType = typeof(Resources.Template))]
        [Display(Name = "Stepper_Demo_FirstName_Label", Description = "Stepper_Demo_FirstName_Hint", ResourceType = typeof(Resources.Template))]
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        [Required(
            ErrorMessageResourceName = "Stepper_Demo_LastName_Required",
            ErrorMessageResourceType = typeof(Resources.Template))]
        [Display(Name = "Stepper_Demo_LastName_Label", Description = "Stepper_Demo_LastName_Hint", ResourceType = typeof(Resources.Template))]
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the selected province or territory.
        /// </summary>
        [Display(Name = "Stepper_Demo_Province_Label", ResourceType = typeof(Resources.Template))]
        public string? Province { get; set; }

        /// <summary>
        /// Gets or sets the selected contact options.
        /// </summary>
        [Display(Name = "Stepper_Demo_ContactOptions_Label", Description = "Stepper_Demo_ContactOptions_Hint", ResourceType = typeof(Resources.Template))]
        public IEnumerable<string>? ContactOptions { get; set; }

        /// <summary>
        /// Available contact options.
        /// </summary>
        public IEnumerable<SelectListItem> ContactOptionsList { get; set; } =
        [
            new() { Value = "email", Text = Resources.Template.Stepper_Demo_ContactOptions_Email },
            new() { Value = "sms", Text = Resources.Template.Stepper_Demo_ContactOptions_Sms }
        ];

        /// <summary>
        /// Available province options.
        /// </summary>
        public IEnumerable<SelectListItem> ProvinceList { get; set; } =
        [
            new() { Value = "AB", Text = Resources.Template.Stepper_Demo_Province_Alberta },
            new() { Value = "BC", Text = Resources.Template.Stepper_Demo_Province_BritishColumbia },
            new() { Value = "ON", Text = Resources.Template.Stepper_Demo_Province_Ontario },
            new() { Value = "QC", Text = Resources.Template.Stepper_Demo_Province_Quebec }
        ];

        // ── Step 2: Info ───────────────────────────────────────────────────

        /// <summary>
        /// A short biography of the user (rich text).
        /// </summary>
        [Required(
            ErrorMessageResourceName = "Stepper_Demo_Bio_Required",
            ErrorMessageResourceType = typeof(Resources.Template))]
        [MinLength(10,
            ErrorMessageResourceName = "Stepper_Demo_Bio_MinLength",
            ErrorMessageResourceType = typeof(Resources.Template))]
        [MaxLength(2000,
            ErrorMessageResourceName = "Stepper_Demo_Bio_MaxLength",
            ErrorMessageResourceType = typeof(Resources.Template))]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Form_Bio_Label", Description = "Form_Bio_Hint", ResourceType = typeof(Resources.Components))]
        public string Bio { get; set; } = string.Empty;

        /// <summary>
        /// The user's date of birth (rendered as month / day / year via DateFormat).
        /// Optional; when provided, GCDS date-input validates the full date parts.
        /// </summary>
        [DataType(DataType.Date)]
        [Display(Name = "Form_DateOfBirth_Label", Description = "Form_DateOfBirth_Hint", ResourceType = typeof(Resources.Components))]
        [DateFormat("full")]
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// The country selected by the user.
        /// </summary>
        [Required(
            ErrorMessageResourceName = "Stepper_Demo_Country_Required",
            ErrorMessageResourceType = typeof(Resources.Template))]
        [Display(Name = "Form_Country_Label", Description = "Form_Country_Hint", ResourceType = typeof(Resources.Components))]
        public string? SelectedCountry { get; set; } = "CA";

        /// <summary>
        /// Available country options.
        /// </summary>
        public IEnumerable<SelectListItem> CountryOptions { get; set; } =
        [
            new() { Value = "CA", Text = "Canada" },
            new() { Value = "US", Text = "United States" },
            new() { Value = "FR", Text = "France" },
            new() { Value = "DE", Text = "Germany" }
        ];

        /// <summary>
        /// The gender selected by the user.
        /// </summary>
        [Required(
            ErrorMessageResourceName = "Stepper_Demo_Gender_Required",
            ErrorMessageResourceType = typeof(Resources.Template))]
        [Display(Name = "Form_Gender_Label", Description = "Form_Gender_Hint", ResourceType = typeof(Resources.Components))]
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// Available gender options.
        /// </summary>
        public IEnumerable<SelectListItem> GenderOptions { get; set; } =
        [
            new() { Value = "Male", Text = "Male" },
            new() { Value = "Female", Text = "Female" },
            new() { Value = "Other", Text = "Other" }
        ];

        // ── Step 3: Details ────────────────────────────────────────────────

        /// <summary>
        /// The user's website URL. Accepts common formats with or without a protocol
        /// (example.com, www.example.com, http(s)://…).
        /// </summary>
        [Required(
            ErrorMessageResourceName = "Stepper_Demo_Website_Required",
            ErrorMessageResourceType = typeof(Resources.Template))]
        [RegularExpression(
            @"^(?:https?:\/\/)?(?:www\.)?(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}(?::\d{1,5})?(?:\/[^\s]*)?$",
            ErrorMessageResourceName = "Stepper_Demo_Website_Invalid",
            ErrorMessageResourceType = typeof(Resources.Template))]
        [Display(Name = "Stepper_Demo_Website_Label", Description = "Stepper_Demo_Website_Hint", ResourceType = typeof(Resources.Template))]
        public string? Website { get; set; }

        /// <summary>
        /// The user's age.
        /// </summary>
        [Required(
            ErrorMessageResourceName = "Stepper_Demo_Age_Required",
            ErrorMessageResourceType = typeof(Resources.Template))]
        [Range(18, 100,
            ErrorMessageResourceName = "Stepper_Demo_Age_Range",
            ErrorMessageResourceType = typeof(Resources.Template))]
        [Display(Name = "Form_Age_Label", ResourceType = typeof(Resources.Components))]
        public int? Age { get; set; }

        // ── Step 5: Submit ─────────────────────────────────────────────────

        /// <summary>
        /// Indicates whether the user agrees to the terms.
        /// Range(true) is required because [Required] on bool does not fail when unchecked (false).
        /// </summary>
        [Range(typeof(bool), "true", "true",
            ErrorMessageResourceName = "Stepper_Demo_AgreeToTerms_Required",
            ErrorMessageResourceType = typeof(Resources.Template))]
        [Display(Name = "Form_AgreeToTerms_Label", Description = "Form_AgreeToTerms_Hint", ResourceType = typeof(Resources.Components))]
        public bool AgreeToTerms { get; set; }
    }
}
