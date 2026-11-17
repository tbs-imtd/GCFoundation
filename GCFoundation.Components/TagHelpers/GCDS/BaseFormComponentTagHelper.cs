using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// A base class for form component tag helpers, providing common properties and functionality for form inputs.
    /// </summary>
    public abstract class BaseFormComponentTagHelper : BaseFormFieldTagHelper
    {
        /// <summary>
        /// If true, the field is focused when the component renders (GCDS <c>autofocus</c>).
        /// </summary>
        public bool Autofocus { get; set; }

        /// <summary>
        /// The ID of the form the field belongs to, when it is outside that form (GCDS <c>form</c>).
        /// </summary>
        public string? Form { get; set; }

        /// <summary>
        /// The error message associated with the input field, if any.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// The event on which validation occurs. Defaults to "blur".
        /// </summary>
        public string ValidateOn { get; set; } = "blur";

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            FormFieldContext field = ResolveFormField();

            AddAttributeIfNotNull(output, "name", field.Name);
            AddAttributeIfNotNull(output, "hint", field.Hint);
            AddAttributeIfNotNull(output, "lang", Lang);
            AddAttributeIfNotNull(output, "value", field.Value);

            AddBooleanAttribute(output, "disabled", field.Disabled);
            AddBooleanAttribute(output, "autofocus", Autofocus);
            AddAttributeIfNotNull(output, "form", Form);
            if (field.Required)
                AddAttributeIfNotNull(output, "required", field.Required);
            AddAttributeIfNotNull(output, "validate-on", ValidateOn);

            string? errorMessage = ErrorMessage ?? ResolveModelStateError(field.Name);
            AddAttributeIfNotNull(output, "error-message", errorMessage);

            base.Process(context, output);
        }
    }
}