using GCFoundation.Common.Utilities;
using GCFoundation.Components.Attributes;
using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// A tag helper for rendering input elements for different data types (e.g., text, date, checkbox, text area). 
    /// It supports automatic binding to model properties and validation, and it dynamically chooses the appropriate input tag based on the property type.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-input for=&quot;@Model.Email&quot;&gt;
    /// &lt;/fdcp-input&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-input", Attributes = "for")]
    [HtmlTargetElement("fdcp-input", Attributes = "name")]
    public class FDCPInputTagHelper : FDCPBaseFormComponentTagHelper
    {
        /// <summary>
        /// Label text for the select. Used when <c>for</c> is not specified,
        /// or overrides the model display name when <c>for</c> is specified.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets the maximum value (GCDS <c>max</c>).
        /// When not specified, it is taken from the <see cref="RangeAttribute"/> of the bound property for number inputs.
        /// </summary>
        [HtmlAttributeName("max")]
        public string? Max { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of characters (GCDS <c>maxlength</c>).
        /// When not specified, it is taken from the <see cref="MaxLengthAttribute"/> of the bound property for text inputs and text areas.
        /// </summary>
        [HtmlAttributeName("maxlength")]
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the minimum value (GCDS <c>min</c>).
        /// When not specified, it is taken from the <see cref="RangeAttribute"/> of the bound property for number inputs.
        /// </summary>
        [HtmlAttributeName("min")]
        public string? Min { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of characters (GCDS <c>minlength</c>).
        /// When not specified, it is taken from the <see cref="MinLengthAttribute"/> of the bound property for text inputs and text areas.
        /// </summary>
        [HtmlAttributeName("minlength")]
        public int? MinLength { get; set; }

        /// <summary>
        /// Gets or sets the type of the input element.
        /// </summary>
        public InputType? Type { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            FormFieldContext field = ResolveFormField(new FormFieldResolveOptions
            {
                Label = Label
            });

            var inputType = ResolveInputType();
            switch (inputType)
            {
                case InputType.checkbox:
                    output.TagName = "gcds-checkbox";
                    output.TagMode = TagMode.StartTagAndEndTag;

                    AddAttributeIfNotNull(output, "label", field.Label);
                    AddAttributeIfNotNull(output, "checkbox-id", field.Id);
                    AddAttributeIfNotNull(output, "value", field.Value ?? string.Empty);
                    break;
                case InputType.date:
                    output.TagName = "gcds-date-input";
                    output.TagMode = TagMode.StartTagAndEndTag;

                    AddAttributeIfNotNull(output, "type", "date");
                    AddAttributeIfNotNull(output, "legend", field.Label);
                    AddAttributeIfNotNull(output, "format", ResolveDateFormat());
                    AddAttributeIfNotNull(output, "min", ResolveMin(inputType));
                    AddAttributeIfNotNull(output, "max", ResolveMax(inputType));

                    // Ensure the value attribute is in expected format by gcds-date-input (YYYY-MM-DD).
                    if (For?.Model is DateTime dateValue)
                    {
                        AddAttributeIfNotNull(output, "value", dateValue.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty);
                    }
                    break;
                case InputType.textArea:
                    output.TagName = "gcds-textarea";
                    output.TagMode = TagMode.StartTagAndEndTag;

                    AddAttributeIfNotNull(output, "label", field.Label);
                    AddAttributeIfNotNull(output, "textarea-id", field.Id);
                    AddAttributeIfNotNull(output, "minlength", ResolveMinLength(inputType));
                    AddAttributeIfNotNull(output, "maxlength", ResolveMaxLength(inputType));
                    AddAttributeIfNotNull(output, "value", field.Value ?? string.Empty);
                    break;
                case InputType.email:
                case InputType.number:
                case InputType.password:
                case InputType.search:
                case InputType.tel:
                case InputType.text:
                case InputType.url:
                default:
                    output.TagName = "gcds-input";
                    output.TagMode = TagMode.StartTagAndEndTag;

                    AddAttributeIfNotNull(output, "type", inputType);
                    AddAttributeIfNotNull(output, "label", field.Label);
                    AddAttributeIfNotNull(output, "input-id", field.Id);

                    if (inputType == InputType.number)
                    {
                        AddAttributeIfNotNull(output, "min", ResolveMin(inputType));
                        AddAttributeIfNotNull(output, "max", ResolveMax(inputType));
                    }

                    AddAttributeIfNotNull(output, "minlength", ResolveMinLength(inputType));
                    AddAttributeIfNotNull(output, "maxlength", ResolveMaxLength(inputType));
                    AddAttributeIfNotNull(output, "value", field.Value ?? string.Empty);
                    break;
            }

            base.Process(context, output);
        }

        #region Resolve methods
        /// <summary>
        /// Retrieves the date format to be provided as defined in the data attributes of the model.
        /// </summary>
        /// <returns>The format of date that should be expected by the input field.</returns>
        protected string ResolveDateFormat()
        {
            if (PropertyInfo != null)
            {
                DateFormatAttribute? formatAttr = PropertyInfo.GetCustomAttribute<DateFormatAttribute>();
                if (formatAttr != null)
                {
                    return formatAttr.Format;
                }
            }

            return "full";
        }

        /// <summary>
        /// Retrieves the appropriate input type (e.g., text, email, password, date, checkbox) based on the Type attribute, the data type and/or the property type.
        /// </summary>
        /// <returns>The type of input that should be rendered.</returns>
        protected InputType ResolveInputType()
        {
            if (Type.HasValue)
                return Type.Value;

            if (DataTypeAttribute != null)
                return DataTypeAttribute.DataType switch
                {
                    DataType.Date => InputType.date,
                    DataType.DateTime => InputType.date,
                    DataType.EmailAddress => InputType.email,
                    DataType.ImageUrl => InputType.url,
                    DataType.MultilineText => InputType.textArea,
                    DataType.Password => InputType.password,
                    DataType.PhoneNumber => InputType.tel,
                    DataType.Url => InputType.url,
                    _ => InputType.text
                };

            // Ensure PropertyInfo is not null before accessing its PropertyType
            if (PropertyInfo == null)
            {
                // Return a default value or handle this case appropriately
                return InputType.text;
            }

            Type propertyType = PropertyInfo.PropertyType;
            Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (propertyType == typeof(bool))
                return InputType.checkbox;
            if (underlyingType == typeof(int) ||
                underlyingType == typeof(decimal) ||
                underlyingType == typeof(double) ||
                underlyingType == typeof(float))
                return InputType.number;

            return InputType.text;
        }

        /// <summary>
        /// Retrieves the minimum value from the Min attribute, or from the <see cref="RangeAttribute"/> of the bound property for number inputs.
        /// </summary>
        /// <param name="inputType">The resolved input type. GCDS only supports a range minimum on number inputs.</param>
        /// <returns>The minimum value, or null when none is defined.</returns>
        protected string? ResolveMin(InputType inputType)
        {
            if (Min != null || inputType != InputType.number)
                return Min;

            return RangeAttribute == null ? null : Convert.ToString(RangeAttribute.Minimum, System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Retrieves the maximum value from the Max attribute, or from the <see cref="RangeAttribute"/> of the bound property for number inputs.
        /// </summary>
        /// <param name="inputType">The resolved input type. GCDS only supports a range maximum on number inputs.</param>
        /// <returns>The maximum value, or null when none is defined.</returns>
        protected string? ResolveMax(InputType inputType)
        {
            if (Max != null || inputType != InputType.number)
                return Max;

            return RangeAttribute == null ? null : Convert.ToString(RangeAttribute.Maximum, System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Retrieves the minimum length from the MinLength attribute, or from the <see cref="MinLengthAttribute"/>
        /// (then <see cref="StringLengthAttribute"/>) of the bound property for text inputs and text areas.
        /// </summary>
        /// <param name="inputType">The resolved input type. Length limits do not apply to checkboxes and date inputs.</param>
        /// <returns>The minimum length, or null when none is defined.</returns>
        protected int? ResolveMinLength(InputType inputType)
        {
            if (MinLength.HasValue || inputType == InputType.checkbox || inputType == InputType.date)
                return MinLength;

            int? minLength = MinLengthAttribute?.Length ?? StringLengthAttribute?.MinimumLength;
            return minLength > 0 ? minLength : null;
        }

        /// <summary>
        /// Retrieves the maximum length from the MaxLength attribute, or from the <see cref="StringLengthAttribute"/>
        /// (then <see cref="MaxLengthAttribute"/>) of the bound property for text inputs and text areas.
        /// </summary>
        /// <param name="inputType">The resolved input type. Length limits do not apply to checkboxes and date inputs.</param>
        /// <returns>The maximum length, or null when none is defined.</returns>
        protected int? ResolveMaxLength(InputType inputType)
        {
            if (MaxLength.HasValue || inputType == InputType.checkbox || inputType == InputType.date)
                return MaxLength;

            // MaxLengthAttribute uses -1 when no length is given (database maximum), which has no HTML equivalent.
            int? maxLength = StringLengthAttribute?.MaximumLength ?? MaxLengthAttribute?.Length;
            return maxLength > 0 ? maxLength : null;
        }
        #endregion Resolve methods
    }
}