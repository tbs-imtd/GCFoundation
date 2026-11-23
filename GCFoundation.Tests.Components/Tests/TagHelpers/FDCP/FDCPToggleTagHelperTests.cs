using GCFoundation.Components.TagHelpers.FDCP;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.FDCP
{
    public class FDCPToggleTagHelperTests
    {
        private readonly TagHelperContext _context;

        public FDCPToggleTagHelperTests()
        {
            _context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test");
        }

        [Fact]
        public void Process_WithName_RendersSwitchLabelAndHiddenFalseValue()
        {
            var output = CreateOutput();
            var tagHelper = CreateTagHelper();
            tagHelper.Name = "notifications";
            tagHelper.Label = "Email notifications";

            tagHelper.Process(_context, output);
            string content = output.Content.GetContent();

            Assert.Equal("div", output.TagName);
            Assert.Equal("fdcp-toggle", output.Attributes["class"].Value?.ToString());
            Assert.True(output.Attributes.ContainsName("data-fdcp-toggle"));
            Assert.Contains("<label class=\"fdcp-toggle__label gcds-label\" for=\"notifications\">Email notifications</label>", content);
            Assert.Contains("type=\"checkbox\" role=\"switch\"", content);
            Assert.Contains("id=\"notifications\" name=\"notifications\" value=\"true\"", content);
            Assert.Contains("<input type=\"hidden\" name=\"notifications\" value=\"false\" />", content);
            Assert.DoesNotContain(" checked", content);
            Assert.DoesNotContain("aria-describedby", content);
            Assert.DoesNotContain("aria-invalid", content);
            Assert.DoesNotContain("<gcds-error-message", content);
            Assert.True(
                content.IndexOf("role=\"switch\"", StringComparison.Ordinal) <
                content.IndexOf("type=\"hidden\"", StringComparison.Ordinal));
        }

        [Fact]
        public void Process_WithCheckedAttribute_RendersCheckedSwitch()
        {
            var output = CreateOutput();
            var tagHelper = CreateTagHelper();
            tagHelper.Name = "notifications";
            tagHelper.Label = "Email notifications";
            tagHelper.Checked = true;

            tagHelper.Process(_context, output);

            Assert.Contains("value=\"true\" checked", output.Content.GetContent());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Process_WithForBinding_UsesModelValueAndDisplayMetadata(bool isOn)
        {
            var output = CreateOutput();
            var tagHelper = CreateTagHelper();
            tagHelper.For = CreateModelExpression(nameof(TestModel.EmailNotifications), new TestModel { EmailNotifications = isOn });

            tagHelper.Process(_context, output);
            string content = output.Content.GetContent();

            Assert.Contains("name=\"EmailNotifications\"", content);
            Assert.Contains(">Email notifications</label>", content);
            Assert.Contains("Send me updates by email", content);
            Assert.Equal(isOn, content.Contains("value=\"true\" checked", StringComparison.Ordinal));
        }

        [Fact]
        public void Process_WithCheckedFalse_OverridesBoundModelValue()
        {
            var output = CreateOutput();
            var tagHelper = CreateTagHelper();
            tagHelper.For = CreateModelExpression(nameof(TestModel.EmailNotifications), new TestModel { EmailNotifications = true });
            tagHelper.Checked = false;

            tagHelper.Process(_context, output);

            Assert.DoesNotContain(" checked", output.Content.GetContent());
        }

        [Fact]
        public void Process_WithAttemptedModelStateValue_RestoresPostedState()
        {
            var output = CreateOutput();
            var tagHelper = CreateTagHelper();
            tagHelper.For = CreateModelExpression(nameof(TestModel.EmailNotifications), new TestModel { EmailNotifications = false });
            tagHelper.ViewContext.ModelState.SetModelValue("EmailNotifications", new[] { "true", "false" }, "true,false");

            tagHelper.Process(_context, output);

            Assert.Contains("value=\"true\" checked", output.Content.GetContent());
        }

        [Fact]
        public void Process_RendersLocalizedStateTextHiddenFromAssistiveTechnology()
        {
            var previousCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentUICulture = new CultureInfo("fr-CA");
                var output = CreateOutput();
                var tagHelper = CreateTagHelper();
                tagHelper.Name = "notifications";
                tagHelper.Label = "Notifications";

                tagHelper.Process(_context, output);
                string content = System.Net.WebUtility.HtmlDecode(output.Content.GetContent());

                Assert.Contains("<span class=\"fdcp-toggle__state\" aria-hidden=\"true\" hidden>", content);
                Assert.Contains("<span class=\"fdcp-toggle__state-on\">Activé</span>", content);
                Assert.Contains("<span class=\"fdcp-toggle__state-off\">Désactivé</span>", content);
            }
            finally
            {
                CultureInfo.CurrentUICulture = previousCulture;
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Process_LeavesStateToNativeCheckedAndKeepsLabelConstant(bool isOn)
        {
            var output = CreateOutput();
            var tagHelper = CreateTagHelper();
            tagHelper.Name = "notifications";
            tagHelper.Label = "Email notifications";
            tagHelper.Checked = isOn;

            tagHelper.Process(_context, output);
            string content = output.Content.GetContent();

            Assert.DoesNotContain("aria-checked", content);
            Assert.DoesNotContain("aria-pressed", content);
            Assert.DoesNotContain("aria-label", content);
            Assert.Contains("<label class=\"fdcp-toggle__label gcds-label\" for=\"notifications\">Email notifications</label>", content);
            Assert.Equal(1, content.Split("role=\"switch\"").Length - 1);
        }

        [Fact]
        public void Process_WithCustomStateText_EncodesValues()
        {
            var output = CreateOutput();
            var tagHelper = CreateTagHelper();
            tagHelper.Name = "visibility";
            tagHelper.Label = "Profile <visibility>";
            tagHelper.OnText = "Public & visible";
            tagHelper.OffText = "<Private>";

            tagHelper.Process(_context, output);
            string content = output.Content.GetContent();

            Assert.Contains("Profile &lt;visibility&gt;", content);
            Assert.Contains("Public &amp; visible", content);
            Assert.Contains("&lt;Private&gt;", content);
            Assert.DoesNotContain("<Private>", content);
        }

        [Fact]
        public void Process_WithHint_LinksHintToSwitch()
        {
            var output = CreateOutput();
            var tagHelper = CreateTagHelper();
            tagHelper.Name = "notifications";
            tagHelper.Label = "Email notifications";
            tagHelper.Hint = "You can change this later.";

            tagHelper.Process(_context, output);
            string content = output.Content.GetContent();

            Assert.Contains("<gcds-hint hint-id=\"notifications_hint\" id=\"notifications_hint\">You can change this later.</gcds-hint>", content);
            Assert.Contains("aria-describedby=\"notifications_hint\"", content);
        }

        [Fact]
        public void Process_WithServerError_RendersErrorBeforeSwitchAndMarksSwitchInvalid()
        {
            var output = CreateOutput();
            var tagHelper = CreateTagHelper();
            tagHelper.Name = "notifications";
            tagHelper.Label = "Email notifications";
            tagHelper.Hint = "You can change this later.";
            tagHelper.ViewContext.ModelState.AddModelError("notifications", "Notifications cannot be changed right now.");

            tagHelper.Process(_context, output);
            string content = output.Content.GetContent();

            Assert.Contains("<gcds-error-message id=\"notifications_error\" message-id=\"notifications_error\">Notifications cannot be changed right now.</gcds-error-message>", content);
            Assert.Contains("aria-describedby=\"notifications_hint notifications_error\"", content);
            Assert.Contains("aria-invalid=\"true\"", content);
            Assert.True(
                content.IndexOf("<gcds-hint", StringComparison.Ordinal) <
                content.IndexOf("<gcds-error-message", StringComparison.Ordinal));
            Assert.True(
                content.IndexOf("<gcds-error-message", StringComparison.Ordinal) <
                content.IndexOf("role=\"switch\"", StringComparison.Ordinal));
        }

        [Fact]
        public void Process_WhenDisabled_DisablesSwitchAndOmitsHiddenValue()
        {
            var output = CreateOutput();
            var tagHelper = CreateTagHelper();
            tagHelper.Name = "notifications";
            tagHelper.Label = "Email notifications";
            tagHelper.Checked = true;
            tagHelper.Disabled = true;

            tagHelper.Process(_context, output);
            string content = output.Content.GetContent();

            Assert.Contains("value=\"true\" checked disabled", content);
            Assert.DoesNotContain("type=\"hidden\"", content);
        }

        [Fact]
        public void Process_WithNestedName_SanitizesIdButKeepsPostedName()
        {
            var output = CreateOutput();
            var tagHelper = CreateTagHelper();
            tagHelper.Name = "Settings.EmailNotifications";
            tagHelper.Label = "Email notifications";

            tagHelper.Process(_context, output);
            string content = output.Content.GetContent();

            Assert.Contains("for=\"Settings_EmailNotifications\"", content);
            Assert.Contains("id=\"Settings_EmailNotifications\" name=\"Settings.EmailNotifications\"", content);
        }

        [Fact]
        public void Process_WithoutForOrName_ThrowsInvalidOperationException()
        {
            var tagHelper = CreateTagHelper();

            var ex = Assert.Throws<InvalidOperationException>(() => tagHelper.Process(_context, CreateOutput()));
            Assert.Contains("Either 'for' or 'name' must be specified", ex.Message);
        }

        private static FDCPToggleTagHelper CreateTagHelper()
        {
            return new FDCPToggleTagHelper
            {
                ViewContext = new ViewContext()
            };
        }

        private static TagHelperOutput CreateOutput()
        {
            return new TagHelperOutput(
                "fdcp-toggle",
                new TagHelperAttributeList(),
                (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        }

        private static ModelExpression CreateModelExpression(string propertyName, TestModel model)
        {
            var modelExplorer = new EmptyModelMetadataProvider().GetModelExplorerForType(typeof(TestModel), model);
            return new ModelExpression(propertyName, modelExplorer.GetExplorerForProperty(propertyName));
        }

        public class TestModel
        {
            [Display(Name = "Email notifications", Description = "Send me updates by email")]
            public bool EmailNotifications { get; set; }
        }
    }
}
