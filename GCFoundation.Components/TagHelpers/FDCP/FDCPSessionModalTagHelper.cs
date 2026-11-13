using GCFoundation.Components.Resources;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// A TagHelper that renders a session modal for managing session timeouts and reminders.
    /// It extends the <see cref="FDCPModalTagHelper"/> and adds additional attributes for session handling.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-session-modal modal-id=&quot;session-extend-modal&quot; title=&quot;Your session is about to expire&quot; static-backdrop hide-close-button session-timeout=&quot;3600&quot; reminder-time=&quot;300&quot; refresh-url=&quot;/authentication/refresh&quot; logout-url=&quot;/authentication/logout&quot;&gt;
    ///     &lt;div slot=&quot;body&quot;&gt;
    ///         &lt;gcds-text&gt;Your session will expire soon. Stay signed in or log out.&lt;/gcds-text&gt;
    ///     &lt;/div&gt;
    ///     &lt;div slot=&quot;footer&quot;&gt;
    ///         &lt;gcds-button button-id=&quot;session-extend-refresh-btn&quot; button-role=&quot;primary&quot;&gt;Stay signed in&lt;/gcds-button&gt;
    ///         &lt;gcds-button button-id=&quot;session-extend-logout-btn&quot; button-role=&quot;danger&quot; type=&quot;link&quot; href=&quot;/authentication/logout&quot;&gt;Log out&lt;/gcds-button&gt;
    ///     &lt;/div&gt;
    /// &lt;/fdcp-session-modal&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-session-modal")]
    public class FDCPSessionModalTagHelper(IStringLocalizer<Modal> localizer) : FDCPModalTagHelper(localizer) 
    {
        /// <summary>
        /// The session timeout in seconds. This is the time after which the session will expire.
        /// </summary>
        public int SessionTimeout { get; set; }

        /// <summary>
        /// The time in seconds before the session timeout, at which a reminder should be shown to the user.
        /// </summary>
        public int ReminderTime { get; set; }

        /// <summary>
        /// The URL to refresh the session. This is called when the session is about to expire.
        /// </summary>
        public Uri RefreshURL { get; set; } = default!;

        /// <summary>
        /// The URL to log out the user. This is called when the session expires or the user chooses to log out.
        /// </summary>
        public Uri LogoutURL { get; set; } = default!;

        /// <inheritdoc/>
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            output.Attributes.SetAttribute("data-session-timeout", SessionTimeout);
            output.Attributes.SetAttribute("data-reminder-time", ReminderTime);
            output.Attributes.SetAttribute("data-refresh", RefreshURL);
            output.Attributes.SetAttribute("data-logout", LogoutURL);


            return base.ProcessAsync(context, output);
        }
    }
}
