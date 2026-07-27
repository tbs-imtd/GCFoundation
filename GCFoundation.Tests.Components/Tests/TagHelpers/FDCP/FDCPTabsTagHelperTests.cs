using GCFoundation.Components.TagHelpers.FDCP;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.FDCP
{
    public class FDCPTabsTagHelperTests
    {
        [Fact]
        public async Task ProcessAsync_WithTabs_RendersAriaTabsAndPanels()
        {
            // Arrange
            var tagHelper = new FDCPTabsTagHelper
            {
                Id = "profile-tabs",
                Label = "Profile sections"
            };
            var output = CreateOutput(
                """
                <fdcp-tab title="Overview"><p>Overview content.</p></fdcp-tab>
                <fdcp-tab title="Settings"><p>Settings content.</p></fdcp-tab>
                """);

            // Act
            await tagHelper.ProcessAsync(CreateContext(), output);

            // Assert
            Assert.Equal("div", output.TagName);
            Assert.Equal("profile-tabs", output.Attributes["id"].Value);
            Assert.Equal("fdcp-tabs", output.Attributes["class"].Value);
            Assert.Equal("true", output.Attributes["data-fdcp-tabs"].Value);
            Assert.Equal("Loading tab content...", output.Attributes["data-loading-text"].Value);
            Assert.Equal("Unable to load this tab. Please try again.", output.Attributes["data-load-error-text"].Value);

            var content = output.Content.GetContent();
            Assert.Contains("role='tablist' aria-label='Profile sections'", content);
            Assert.Contains("id='profile-tabs-tab-1-tab' role='tab' aria-selected='true' aria-controls='profile-tabs-tab-1-panel' tabindex='0'", content);
            Assert.Contains("id='profile-tabs-tab-2-tab' role='tab' aria-selected='false' aria-controls='profile-tabs-tab-2-panel' tabindex='-1'", content);
            Assert.Contains("<div class='fdcp-tabs__panel' id='profile-tabs-tab-1-panel' role='tabpanel' aria-labelledby='profile-tabs-tab-1-tab' tabindex='0'>", content);
            Assert.Contains("<div class='fdcp-tabs__panel' id='profile-tabs-tab-2-panel' role='tabpanel' aria-labelledby='profile-tabs-tab-2-tab' tabindex='0' hidden>", content);
            Assert.Contains("<p>Overview content.</p>", content);
            Assert.Contains("<p>Settings content.</p>", content);
        }

        [Fact]
        public async Task ProcessAsync_WithActiveChild_SelectsActiveTab()
        {
            // Arrange
            var tagHelper = new FDCPTabsTagHelper { Id = "tabs" };
            var output = CreateOutput(
                """
                <fdcp-tab title="First"><p>First content.</p></fdcp-tab>
                <fdcp-tab title="Second" active="true"><p>Second content.</p></fdcp-tab>
                """);

            // Act
            await tagHelper.ProcessAsync(CreateContext(), output);

            // Assert
            var content = output.Content.GetContent();
            Assert.Contains("id='tabs-tab-1-tab' role='tab' aria-selected='false'", content);
            Assert.Contains("id='tabs-tab-2-tab' role='tab' aria-selected='true'", content);
            Assert.Contains("id='tabs-tab-1-panel' role='tabpanel' aria-labelledby='tabs-tab-1-tab' tabindex='0' hidden", content);
            Assert.Contains("id='tabs-tab-2-panel' role='tabpanel' aria-labelledby='tabs-tab-2-tab' tabindex='0'", content);
        }

        [Fact]
        public async Task ProcessAsync_WithSelectedIndex_SelectsIndexedTab()
        {
            // Arrange
            var tagHelper = new FDCPTabsTagHelper
            {
                Id = "tabs",
                SelectedIndex = 1
            };
            var output = CreateOutput(
                """
                <fdcp-tab title="First" active="true"><p>First content.</p></fdcp-tab>
                <fdcp-tab title="Second"><p>Second content.</p></fdcp-tab>
                """);

            // Act
            await tagHelper.ProcessAsync(CreateContext(), output);

            // Assert
            var content = output.Content.GetContent();
            Assert.Contains("id='tabs-tab-1-tab' role='tab' aria-selected='false'", content);
            Assert.Contains("id='tabs-tab-2-tab' role='tab' aria-selected='true'", content);
        }

        [Fact]
        public async Task ProcessAsync_WithChildIds_UsesChildIdsForTabAndPanel()
        {
            // Arrange
            var tagHelper = new FDCPTabsTagHelper { Id = "tabs" };
            var output = CreateOutput(
                """
                <fdcp-tab id="overview" title="Overview"><p>Overview content.</p></fdcp-tab>
                """);

            // Act
            await tagHelper.ProcessAsync(CreateContext(), output);

            // Assert
            var content = output.Content.GetContent();
            Assert.Contains("id='tabs-overview-tab' role='tab' aria-selected='true' aria-controls='tabs-overview-panel'", content);
            Assert.Contains("id='tabs-overview-panel' role='tabpanel' aria-labelledby='tabs-overview-tab'", content);
        }

        [Fact]
        public async Task ProcessAsync_WithLoadUrl_RendersDataLoadUrlOnTab()
        {
            // Arrange
            var tagHelper = new FDCPTabsTagHelper { Id = "tabs" };
            var output = CreateOutput(
                """
                <fdcp-tab id="history" title="History" load-url="/profile/history"><p>Loading history...</p></fdcp-tab>
                """);

            // Act
            await tagHelper.ProcessAsync(CreateContext(), output);

            // Assert
            var content = output.Content.GetContent();
            Assert.Contains("id='tabs-history-tab' role='tab' aria-selected='true' aria-controls='tabs-history-panel' tabindex='0' data-load-url='/profile/history'", content);
            Assert.Contains("id='tabs-history-panel' role='tabpanel' aria-labelledby='tabs-history-tab' tabindex='0' aria-live='polite' aria-atomic='true'", content);
            Assert.Contains("<p>Loading history...</p>", content);
        }

        [Fact]
        public async Task ProcessAsync_WithDuplicateChildIds_GeneratesUniqueScopedIds()
        {
            // Arrange
            var tagHelper = new FDCPTabsTagHelper { Id = "account" };
            var output = CreateOutput(
                """
                <fdcp-tab id="details" title="First"><p>First content.</p></fdcp-tab>
                <fdcp-tab id="details" title="Second"><p>Second content.</p></fdcp-tab>
                """);

            // Act
            await tagHelper.ProcessAsync(CreateContext(), output);

            // Assert
            var content = output.Content.GetContent();
            Assert.Contains("id='account-details-tab'", content);
            Assert.Contains("id='account-details-2-tab'", content);
            Assert.Contains("id='account-details-panel'", content);
            Assert.Contains("id='account-details-2-panel'", content);
        }

        [Fact]
        public async Task ProcessAsync_WithNestedTabMarkup_OnlyRendersDirectTabs()
        {
            // Arrange
            var tagHelper = new FDCPTabsTagHelper { Id = "outer" };
            var output = CreateOutput(
                """
                <fdcp-tab title="Outer">
                    <fdcp-tabs>
                        <fdcp-tab title="Nested"><p>Nested content.</p></fdcp-tab>
                    </fdcp-tabs>
                </fdcp-tab>
                """);

            // Act
            await tagHelper.ProcessAsync(CreateContext(), output);

            // Assert
            var content = output.Content.GetContent();
            Assert.Equal(2, content.Split("role='tab'", StringSplitOptions.None).Length);
            Assert.Contains(">Outer</button>", content);
            Assert.DoesNotContain(">Nested</button>", content);
        }

        [Fact]
        public async Task ProcessAsync_WithFocusablePanelContent_DoesNotAddPanelTabIndex()
        {
            // Arrange
            var tagHelper = new FDCPTabsTagHelper { Id = "tabs" };
            var output = CreateOutput("""<fdcp-tab title="Actions"><a href="/next">Next</a></fdcp-tab>""");

            // Act
            await tagHelper.ProcessAsync(CreateContext(), output);

            // Assert
            var content = output.Content.GetContent();
            Assert.Contains("id='tabs-tab-1-panel' role='tabpanel' aria-labelledby='tabs-tab-1-tab'>", content);
            Assert.DoesNotContain("id='tabs-tab-1-panel' role='tabpanel' aria-labelledby='tabs-tab-1-tab' tabindex='0'", content);
        }

        [Fact]
        public async Task ProcessAsync_WithFrenchCulture_LocalizesDefaultsAndClientMessages()
        {
            // Arrange
            using var cultureScope = new CultureScope("fr-CA");
            var tagHelper = new FDCPTabsTagHelper { Id = "tabs" };
            var output = CreateOutput("""<fdcp-tab title="Aperçu"><p>Contenu.</p></fdcp-tab>""");

            // Act
            await tagHelper.ProcessAsync(CreateContext(), output);

            // Assert
            Assert.Equal("Chargement du contenu de l'onglet...", output.Attributes["data-loading-text"].Value);
            Assert.Equal("Impossible de charger cet onglet. Veuillez réessayer.", output.Attributes["data-load-error-text"].Value);
            Assert.Contains("role='tablist' aria-label='Onglets'", output.Content.GetContent());
        }

        [Fact]
        public async Task ProcessAsync_WithEncodedTitle_EncodesTabText()
        {
            // Arrange
            var tagHelper = new FDCPTabsTagHelper { Id = "tabs" };
            var output = CreateOutput("""<fdcp-tab title="&lt;Overview&gt;"><p>Content.</p></fdcp-tab>""");

            // Act
            await tagHelper.ProcessAsync(CreateContext(), output);

            // Assert
            var content = output.Content.GetContent();
            Assert.Contains("&lt;Overview&gt;", content);
            Assert.DoesNotContain("><Overview></button>", content);
        }

        [Fact]
        public async Task TabProcessAsync_EmitsParentPlaceholder()
        {
            // Arrange
            var tagHelper = new FDCPTabTagHelper
            {
                Id = "overview",
                Title = "Overview",
                Active = true,
                LoadUrl = new Uri("/profile/overview", UriKind.Relative)
            };
            var output = CreateOutput("<p>Overview content.</p>", "fdcp-tab");

            // Act
            await tagHelper.ProcessAsync(CreateContext("fdcp-tab"), output);

            // Assert
            Assert.Equal("div", output.TagName);
            Assert.Equal("true", output.Attributes["data-fdcp-tab"].Value);
            Assert.Equal("Overview", output.Attributes["data-title"].Value);
            Assert.Equal("true", output.Attributes["data-active"].Value);
            Assert.Equal("overview", output.Attributes["data-id"].Value);
            Assert.Equal("/profile/overview", output.Attributes["data-load-url"].Value);
            Assert.Equal("<p>Overview content.</p>", output.Content.GetContent());
        }

        [Fact]
        public async Task ProcessAsync_WithNullOutput_ThrowsArgumentNullException()
        {
            // Arrange
            var tagHelper = new FDCPTabsTagHelper();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => tagHelper.ProcessAsync(CreateContext(), null!));
        }

        private static TagHelperContext CreateContext(string tagName = "fdcp-tabs")
        {
            return new TagHelperContext(
                tagName,
                new TagHelperAttributeList(),
                new Dictionary<object, object?>(),
                "test-id");
        }

        private static TagHelperOutput CreateOutput(string childContent, string tagName = "fdcp-tabs")
        {
            return new TagHelperOutput(
                tagName,
                new TagHelperAttributeList(),
                (result, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetHtmlContent(childContent);
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
        }

        private sealed class CultureScope : IDisposable
        {
            private readonly CultureInfo previousCulture;
            private readonly CultureInfo previousUiCulture;

            public CultureScope(string cultureName)
            {
                previousCulture = CultureInfo.CurrentCulture;
                previousUiCulture = CultureInfo.CurrentUICulture;
                var culture = CultureInfo.GetCultureInfo(cultureName);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }

            public void Dispose()
            {
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUiCulture;
            }
        }
    }
}
