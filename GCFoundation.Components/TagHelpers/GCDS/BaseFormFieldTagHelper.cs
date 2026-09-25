using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// Provides shared model binding, metadata, and validation helpers for form component tag helpers.
    /// </summary>
    public abstract class BaseFormFieldTagHelper : BaseTagHelper
    {
        /// <summary>
        /// Retrieves the <see cref="DataTypeAttribute"/> for the property if available.
        /// </summary>
        protected DataTypeAttribute? DataTypeAttribute
        {
            get
            {
                if (PropertyInfo == null)
                {
                    return null;
                }

                return PropertyInfo.GetCustomAttribute<DataTypeAttribute>();
            }
        }

        /// <summary>
        /// Defines whether the form component is disabled.
        /// </summary>
        public bool? Disabled { get; set; }

        /// <summary>
        /// Binds the tag helper to a model property, enabling validation and data binding.
        /// </summary>
        [HtmlAttributeName("for")]
        public ModelExpression For { get; set; } = default!;

        /// <summary>
        /// A hint providing additional information on how to answer the input.
        /// </summary>
        [HtmlAttributeName("hint")]
        public string? Hint { get; set; }

        /// <summary>
        /// Defines the <strong>id</strong> attribute of the form component.
        /// </summary>
        /// <remarks><em>Overrides the value derived from any bindings.</em></remarks>
        [HtmlAttributeName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Defines the <strong>name</strong> attribute of the form component.
        /// </summary>
        /// <remarks><em>Overrides the value derived from any bindings.</em></remarks>
        [HtmlAttributeName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Retrieves the <see cref="PropertyInfo"/> for the model property bound to this tag helper.
        /// </summary>
        protected PropertyInfo? PropertyInfo
        {
            get
            {
                if (For == null || string.IsNullOrEmpty(For.Metadata.PropertyName))
                {
                    return null;
                }

                return For.Metadata.ContainerType?.GetProperty(For.Metadata.PropertyName);
            }
        }

        /// <summary>
        /// Defines whether the form component is required or not.
        /// </summary>
        /// <remarks><em>Overrides the [Required] data annotation (if applicable).</em></remarks>
        public bool? Required { get; set; }

        /// <summary>
        /// The value of the input field.
        /// </summary>
        [HtmlAttributeName("value")]
        public string? Value { get; set; }

        /// <summary>
        /// Injects the current ViewContext to access ModelState for validation.
        /// </summary>
        [ViewContext]
        public ViewContext ViewContext { get; set; } = default!;

        #region Helper classes
        /// <summary>
        /// Resolved field metadata shared by form component tag helpers.
        /// </summary>
        /// <param name="Name">The form field name.</param>
        /// <param name="Id">The form field id.</param>
        /// <param name="Label">The localized label or legend text.</param>
        /// <param name="Hint">The localized hint text.</param>
        /// <param name="Disabled">Whether the field is disabled.</param>
        /// <param name="Required">Whether the field is required.</param>
        /// <param name="Value">The string representation of the current value, if any.</param>
        /// <param name="Property">The bound model property, when resolved from <c>for</c>.</param>
        /// <param name="Model">The bound model value, when resolved from <c>for</c>.</param>
        protected sealed record FormFieldContext(
            string Name,
            string Id,
            string Label,
            string Hint,
            bool Disabled,
            bool Required,
            string? Value,
            PropertyInfo? Property,
            object? Model);

        /// <summary>
        /// Optional overrides used when resolving form field metadata.
        /// </summary>
        protected sealed class FormFieldResolveOptions
        {
            /// <summary>
            /// Overrides the id derived from model metadata.
            /// </summary>
            public string? Id { get; init; }

            /// <summary>
            /// Overrides the name derived from model metadata.
            /// </summary>
            public string? Name { get; init; }

            /// <summary>
            /// Overrides the label or legend derived from model metadata.
            /// </summary>
            public string? Label { get; init; }

            /// <summary>
            /// Overrides the hint derived from model metadata.
            /// </summary>
            public string? Hint { get; init; }

            /// <summary>
            /// Overrides whether the field is disabled.
            /// </summary>
            public bool? Disabled { get; init; }

            /// <summary>
            /// Overrides the value derived from model binding.
            /// </summary>
            public string? Value { get; init; }
        }
        #endregion Helper classes

        #region Resolve methods
        /// <summary>
        /// Resolves common form field metadata from model binding and optional manual overrides.
        /// </summary>
        /// <param name="options">Optional manual overrides for field metadata.</param>
        /// <returns>The resolved field metadata.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when metadata cannot be resolved from <c>for</c> or manual <c>name</c>.
        /// </exception>
        protected FormFieldContext ResolveFormField(FormFieldResolveOptions? options = null)
        {
            if (TryResolveFormField(out FormFieldContext field, options))
            {
                return field;
            }

            string message = (For != null ? "Missing properties" : "Either 'for' or 'name' must be specified.");
            throw new InvalidOperationException(message);
        }

        /// <summary>
        /// Retrieves the localized label for a property, falling back to the property name if no label is provided.
        /// </summary>
        /// <param name="property">The property to retrieve the label for.</param>
        /// <returns>The localized label for the property.</returns>
        protected static string ResolveLocalizedLabel(PropertyInfo property)
        {
            ArgumentNullException.ThrowIfNull(property, nameof(property));

            var displayAttr = property.GetCustomAttribute<DisplayAttribute>();
            return displayAttr?.GetName() ?? property.Name;
        }

        /// <summary>
        /// Retrieves the localized hint for a property, falling back to an empty string if no hint is provided.
        /// </summary>
        /// <param name="property">The property to retrieve the hint for.</param>
        /// <returns>The localized hint for the property.</returns>
        protected static string ResolveLocalizedHint(PropertyInfo property)
        {
            ArgumentNullException.ThrowIfNull(property, nameof(property));

            var displayAttr = property.GetCustomAttribute<DisplayAttribute>();
            return displayAttr?.GetDescription() ?? string.Empty;
        }

        /// <summary>
        /// Retrieves the first ModelState error message for a field, optionally only after form submission.
        /// </summary>
        protected string? ResolveModelStateError(string fieldName, bool onlyAfterSubmit = true)
        {
            if (onlyAfterSubmit)
            {
                bool formWasSubmitted = ViewContext?.HttpContext?.Request?.Method == "POST" ||
                                        ViewContext?.ModelState?.ErrorCount > 0;

                if (!formWasSubmitted)
                {
                    return null;
                }
            }

            if (ViewContext?.ModelState?.ContainsKey(fieldName) == true &&
                ViewContext.ModelState[fieldName]?.Errors?.Count > 0)
            {
                return ViewContext.ModelState[fieldName]!.Errors[0].ErrorMessage;
            }

            return null;
        }

        /// <summary>
        /// Determines whether the bound field is required from metadata and tag helper overrides.
        /// </summary>
        protected bool ResolveRequired(PropertyInfo? property)
        {
            bool required = false;

            if (For != null)
            {
                required = For.Metadata.ValidatorMetadata.OfType<RequiredAttribute>().Any()
                           || property?.GetCustomAttribute<RequiredAttribute>() != null;
            }

            if (Required.HasValue)
            {
                required = Required.Value;
            }

            return required;
        }

        /// <summary>
        /// Emits GCDS constraint attributes from DataAnnotations on the bound property.
        /// Existing output attributes (markup or helper properties) are left unchanged.
        /// </summary>
        protected void ApplyDataAnnotationConstraints(TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            if (For == null)
                return;

            string tagName = output.TagName ?? string.Empty;
            bool isInput = tagName.Equals("gcds-input", StringComparison.OrdinalIgnoreCase);
            bool isTextarea = tagName.Equals("gcds-textarea", StringComparison.OrdinalIgnoreCase);
            bool isDateInput = tagName.Equals("gcds-date-input", StringComparison.OrdinalIgnoreCase);

            if (!isInput && !isTextarea && !isDateInput)
                return;

            if (isInput || isTextarea)
                ApplyLengthConstraints(output);

            if (isInput)
            {
                ApplyInputTypeFromDataType(output);

                RegularExpressionAttribute? regex = GetValidatorMetadata<RegularExpressionAttribute>();
                if (!string.IsNullOrEmpty(regex?.Pattern))
                    AddConstraintAttributeIfMissing(output, "pattern", regex.Pattern);

                ApplyRangeConstraints(output, dateRange: false);

                if (ResolveReadOnly() && !output.Attributes.ContainsName("readonly"))
                    AddBooleanAttribute(output, "readonly", true);
            }

            if (isDateInput)
                ApplyRangeConstraints(output, dateRange: true);
        }

        private TAttribute? GetValidatorMetadata<TAttribute>() where TAttribute : ValidationAttribute
        {
            if (For == null)
                return null;

            return For.Metadata.ValidatorMetadata.OfType<TAttribute>().FirstOrDefault()
                   ?? PropertyInfo?.GetCustomAttribute<TAttribute>();
        }

        private bool ResolveReadOnly()
        {
            if (For?.Metadata.IsReadOnly == true)
                return true;

            if (PropertyInfo?.GetCustomAttribute<ReadOnlyAttribute>()?.IsReadOnly == true)
                return true;

            EditableAttribute? editable = PropertyInfo?.GetCustomAttribute<EditableAttribute>();
            return editable != null && !editable.AllowEdit;
        }

        private void ApplyInputTypeFromDataType(TagHelperOutput output)
        {
            if (DataTypeAttribute == null)
                return;

            string? inputType = DataTypeAttribute.DataType switch
            {
                DataType.EmailAddress => "email",
                DataType.Password => "password",
                DataType.PhoneNumber => "tel",
                DataType.Url or DataType.ImageUrl => "url",
                _ => null
            };

            AddConstraintAttributeIfMissing(output, "type", inputType);
        }

        private void ApplyLengthConstraints(TagHelperOutput output)
        {
            StringLengthAttribute? stringLength = GetValidatorMetadata<StringLengthAttribute>();
            MaxLengthAttribute? maxLength = GetValidatorMetadata<MaxLengthAttribute>();
            MinLengthAttribute? minLength = GetValidatorMetadata<MinLengthAttribute>();

            int? resolvedMax = stringLength != null ? stringLength.MaximumLength : maxLength?.Length;
            int? resolvedMin = stringLength != null && stringLength.MinimumLength > 0
                ? stringLength.MinimumLength
                : minLength?.Length;

            if (resolvedMax is > 0)
                AddConstraintAttributeIfMissing(output, "maxlength", resolvedMax.Value.ToString(CultureInfo.InvariantCulture));

            if (resolvedMin is > 0)
                AddConstraintAttributeIfMissing(output, "minlength", resolvedMin.Value.ToString(CultureInfo.InvariantCulture));
        }

        private void ApplyRangeConstraints(TagHelperOutput output, bool dateRange)
        {
            RangeAttribute? range = GetValidatorMetadata<RangeAttribute>();
            if (range?.Minimum == null || range.Maximum == null)
                return;

            bool isDate = range.OperandType == typeof(DateTime)
                          || range.Minimum is DateTime
                          || range.Maximum is DateTime;

            if (isDate != dateRange)
                return;

            AddConstraintAttributeIfMissing(output, "min", FormatRangeValue(range.Minimum, isDate));
            AddConstraintAttributeIfMissing(output, "max", FormatRangeValue(range.Maximum, isDate));
        }

        private static string? FormatRangeValue(object value, bool isDate)
        {
            if (isDate)
            {
                if (value is DateTime dateTime)
                    return dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                if (DateTime.TryParse(
                        Convert.ToString(value, CultureInfo.InvariantCulture),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime parsed))
                {
                    return parsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
            }

            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static void AddConstraintAttributeIfMissing(TagHelperOutput output, string attributeName, string? attributeValue)
        {
            if (output.Attributes.ContainsName(attributeName))
                return;

            AddAttributeIfNotNull(output, attributeName, attributeValue);
        }

        /// <summary>
        /// Attempts to resolve common form field metadata from model binding and optional manual overrides.
        /// </summary>
        /// <param name="field">The resolved field metadata when successful.</param>
        /// <param name="options">Optional manual overrides for field metadata.</param>
        /// <returns><c>true</c> when metadata was resolved; otherwise <c>false</c>.</returns>
        private bool TryResolveFormField(out FormFieldContext field, FormFieldResolveOptions? options = null)
        {
            options ??= new FormFieldResolveOptions();

            string? nameOverride = options.Name ?? Name;
            string? idOverride = options.Id ?? Id;
            bool disabled = options.Disabled ?? Disabled ?? false;
            string? hintOverride = options.Hint ?? Hint;

            if (For != null)
            {
                PropertyInfo? property = PropertyInfo;
                string fieldName = nameOverride ?? For.Name;
                string fieldId = idOverride ?? fieldName;
                string? value = options.Value ?? Value ?? For.Model?.ToString();

                if (property != null)
                {
                    field = new FormFieldContext(
                        fieldName,
                        fieldId,
                        options.Label ?? ResolveLocalizedLabel(property),
                        hintOverride ?? ResolveLocalizedHint(property),
                        disabled,
                        ResolveRequired(property),
                        value,
                        property,
                        For.Model);
                    return true;
                }

                field = new FormFieldContext(
                    fieldName,
                    fieldId,
                    options.Label ?? For.Metadata.DisplayName ?? For.Name,
                    hintOverride ?? string.Empty,
                    disabled,
                    ResolveRequired(null),
                    value,
                    null,
                    For.Model);
                return true;
            }

            if (string.IsNullOrWhiteSpace(nameOverride))
            {
                field = default!;
                return false;
            }

            field = new FormFieldContext(
                nameOverride,
                idOverride ?? nameOverride,
                options.Label ?? string.Empty,
                hintOverride ?? string.Empty,
                disabled,
                Required ?? false,
                options.Value ?? Value,
                null,
                null);
            return true;
        }
        #endregion Resolve methods
    }
}