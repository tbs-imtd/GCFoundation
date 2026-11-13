using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// Represents a GC Design System alert that communicates important information to the user.
    /// </summary>
    /// <remarks>
    /// Content placed inside the alert is displayed in its default slot and can include text,
    /// links, buttons, or other custom content.
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;gcds-alert heading=&quot;Service interruption&quot; alert-role=&quot;warning&quot;&gt;
    ///     &lt;p&gt;The service is temporarily unavailable.&lt;/p&gt;
    /// &lt;/gcds-alert&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-alert", Attributes = "heading")]
    public class AlertTagHelper : BaseTagHelper
    {
        /// <summary>
        /// Gets or sets the alert role used to determine the alert's visual style and communicate
        /// the urgency or importance of its message. The default is <see cref="AlertType.info"/>.
        /// </summary>
        public AlertType AlertRole { get; set; } = AlertType.info;

        /// <summary>
        /// Gets or sets the maximum width of the alert content.
        /// The default is <see cref="ContainerSize.full"/>.
        /// </summary>
        public ContainerSize Container { get; set; } = ContainerSize.full;

        /// <summary>
        /// Gets or sets the clear and informative heading that conveys the purpose of the alert.
        /// </summary>
        public required string Heading { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dismiss button is hidden.
        /// The default is <see langword="false"/>, which displays the button.
        /// </summary>
        public bool HideCloseBtn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the alert role icon is hidden.
        /// The default is <see langword="false"/>, which displays the icon.
        /// </summary>
        public bool HideRoleIcon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the alert is fixed at the top of the page.
        /// The default is <see langword="false"/>.
        /// </summary>
        public bool IsFixed { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNull(output, "alert-role", AlertRole);
            AddAttributeIfNotNull(output, "container", Container);
            AddAttributeIfNotNull(output, "heading", Heading);
            AddBooleanAttribute(output, "hide-close-btn", HideCloseBtn);
            AddBooleanAttribute(output, "hide-role-icon", HideRoleIcon);
            AddBooleanAttribute(output, "is-fixed", IsFixed);

            base.Process(context, output);
        }

    }
}
