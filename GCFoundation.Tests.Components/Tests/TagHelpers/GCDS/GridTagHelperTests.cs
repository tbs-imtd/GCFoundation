using GCFoundation.Components.Enums;
using GCFoundation.Components.TagHelpers.GCDS;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.GCDS
{
    public class GridTagHelperTests
    {
        [Fact]
        public void Process_WithAllProperties_SetsAllExpectedAttributes()
        {
            // Arrange
            var helper = new GridTagHelper
            {
                AlignContent = AlignContent.spaceAround,
                AlignItems = AlignItem.baseline,
                Columns = "3",
                ColumnsDesktop = "4",
                ColumnsTablet = "2",
                Container = SizeTypeEmum.full,
                Display = GridDisplay.inlineGrid,
                EqualRowHeight = true,
                Gap = "10",
                GapDesktop = "20",
                GapTablet = "5",
                JustifyContent = AlignContent.spaceBetween,
                JustifyItems = AlignItem.stretch,
                PlaceContent = AlignContent.spaceEvenly,
                PlaceItems = AlignItem.center,
                Tag = "section"
            };

            var context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test-unique-id");

            var output = new TagHelperOutput("gcds-grid",
                new TagHelperAttributeList(),
                (_, __) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            // Act
            helper.Process(context, output);

            // Assert - element
            Assert.Equal("gcds-grid", output.TagName);

            // Assert - attributes with kebab-case conversion
            Assert.Equal("space-around", output.Attributes["align-content"].Value?.ToString());
            Assert.Equal("baseline", output.Attributes["align-items"].Value?.ToString());
            Assert.Equal("space-between", output.Attributes["justify-content"].Value?.ToString());
            Assert.Equal("stretch", output.Attributes["justify-items"].Value?.ToString());
            Assert.Equal("space-evenly", output.Attributes["place-content"].Value?.ToString());
            Assert.Equal("center", output.Attributes["place-items"].Value?.ToString());

            // Assert - string and enum attributes
            Assert.Equal("3", output.Attributes["columns"].Value?.ToString());
            Assert.Equal("4", output.Attributes["columns-desktop"].Value?.ToString());
            Assert.Equal("2", output.Attributes["columns-tablet"].Value?.ToString());
            Assert.Equal("full", output.Attributes["container"].Value?.ToString());
            Assert.Equal("inline-grid", output.Attributes["display"].Value?.ToString());
            Assert.Equal("10", output.Attributes["gap"].Value?.ToString());
            Assert.Equal("20", output.Attributes["gap-desktop"].Value?.ToString());
            Assert.Equal("5", output.Attributes["gap-tablet"].Value?.ToString());

            // Assert - boolean flag attribute
            Assert.True(output.Attributes.ContainsName("equal-row-height"));

            // Assert - tag attribute
            Assert.Equal("section", output.Attributes["tag"].Value?.ToString());
        }

        [Fact]
        public void Process_WithDefaultValues_RendersElementWithDefaultTagAndNoOptionalAttributes()
        {
            // Arrange
            var helper = new GridTagHelper();

            var context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test-unique-id");

            var output = new TagHelperOutput("gcds-grid",
                new TagHelperAttributeList(),
                (_, __) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            // Act
            helper.Process(context, output);

            // Assert - element name remains gcds-grid
            Assert.Equal("gcds-grid", output.TagName);

            // Assert - default tag attribute is applied
            Assert.Equal("div", output.Attributes["tag"].Value?.ToString());

            // Assert - optional attributes are not present when not set
            Assert.False(output.Attributes.ContainsName("align-content"));
            Assert.False(output.Attributes.ContainsName("align-items"));
            Assert.False(output.Attributes.ContainsName("columns"));
            Assert.False(output.Attributes.ContainsName("columns-desktop"));
            Assert.False(output.Attributes.ContainsName("columns-tablet"));
            Assert.False(output.Attributes.ContainsName("container"));
            Assert.False(output.Attributes.ContainsName("display"));
            Assert.False(output.Attributes.ContainsName("gap"));
            Assert.False(output.Attributes.ContainsName("gap-desktop"));
            Assert.False(output.Attributes.ContainsName("gap-tablet"));
            Assert.False(output.Attributes.ContainsName("justify-content"));
            Assert.False(output.Attributes.ContainsName("justify-items"));
            Assert.False(output.Attributes.ContainsName("place-content"));
            Assert.False(output.Attributes.ContainsName("place-items"));
            Assert.False(output.Attributes.ContainsName("equal-row-height"));
        }

        [Fact]
        public void Process_WithEmptyStringValues_DoesNotAddStringAttributes()
        {
            // Arrange
            var helper = new GridTagHelper
            {
                Columns = string.Empty,
                ColumnsDesktop = string.Empty,
                ColumnsTablet = string.Empty,
                Gap = string.Empty,
                GapDesktop = string.Empty,
                GapTablet = string.Empty
            };

            var context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test-unique-id");

            var output = new TagHelperOutput("gcds-grid",
                new TagHelperAttributeList(),
                (_, __) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            // Act
            helper.Process(context, output);

            // Assert
            Assert.False(output.Attributes.ContainsName("columns"));
            Assert.False(output.Attributes.ContainsName("columns-desktop"));
            Assert.False(output.Attributes.ContainsName("columns-tablet"));
            Assert.False(output.Attributes.ContainsName("gap"));
            Assert.False(output.Attributes.ContainsName("gap-desktop"));
            Assert.False(output.Attributes.ContainsName("gap-tablet"));
        }
    }
}


