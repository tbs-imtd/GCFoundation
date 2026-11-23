using GCFoundation.Components.TagHelpers.FDCP;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.FDCP;

public class FDCPPageHeadingTagHelperTests
{
    [Fact]
    public void Process_WithTitleOnly_RendersExpectedOutput()
    {
        // Arrange
        var tagHelper = new FDCPPageHeadingTagHelper
        {
            Title = "Test Title"
        };

        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );

        var output = new TagHelperOutput("fdcp-page-heading",
            new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.Equal("fdcp-page-heading-container", output.Attributes["class"].Value);
        Assert.Contains("<gcds-heading tag='h1' heading-role='light'>Test Title</gcds-heading>", output.Content.GetContent());
        Assert.DoesNotContain("<gcds-text>", output.Content.GetContent());
    }

    [Fact]
    public void Process_WithTitleAndDescription_RendersExpectedOutput()
    {
        // Arrange
        var tagHelper = new FDCPPageHeadingTagHelper
        {
            Title = "Test Title",
            Description = "Test Description"
        };

        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );

        var output = new TagHelperOutput("fdcp-page-heading",
            new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.Equal("fdcp-page-heading-container", output.Attributes["class"].Value);
        Assert.Contains("<gcds-heading tag='h1' heading-role='light'>Test Title</gcds-heading>", output.Content.GetContent());
        Assert.Contains("<gcds-text text-role='light'>Test Description</gcds-text>", output.Content.GetContent());
    }

    [Fact]
    public void Process_WithBackgroundImage_RendersExpectedOutput()
    {
        // Arrange
        var tagHelper = new FDCPPageHeadingTagHelper
        {
            Title = "Test Title",
            Src = "background-image.jpg"
        };

        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );

        var output = new TagHelperOutput("fdcp-page-heading",
            new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.Equal("fdcp-page-heading-container fdcp-page-heading-has-bg", output.Attributes["class"].Value);
        Assert.Equal("background-image.jpg", output.Attributes["data-bg-src"].Value);
    }

    [Fact]
    public void Process_NullOutput_ThrowsArgumentNullException()
    {
        // Arrange
        var tagHelper = new FDCPPageHeadingTagHelper
        {
            Title = "Test Title"
        };

        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => tagHelper.Process(context, null!));
    }

    [Fact]
    public void Process_SizeCompactWithDefaultTextEmphasis_RendersExpectedOutput()
    {
        // Arrange
        var tagHelper = new FDCPPageHeadingTagHelper
        {
            Title = "Test Title",
            Description = "Test Description",
            Size = GCFoundation.Components.Enums.PageHeadingSize.compact,
            TextEmphasis = true
        };

        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );

        var output = new TagHelperOutput("fdcp-page-heading",
            new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.Equal("fdcp-page-heading-container fdcp-page-heading-compact", output.Attributes["class"].Value);
        Assert.Contains("<article class='text-container sm:py-350 py-200 sm:pe-750 pe-450 bg-primary text-light'>", output.Content.GetContent());
        Assert.Contains("<gcds-heading tag='h1' heading-role='light'>Test Title</gcds-heading>", output.Content.GetContent());
        Assert.Contains("<gcds-text text-role='light'>Test Description</gcds-text>", output.Content.GetContent());
    }

    [Fact]
    public void Process_SizeCompactWithTextEmphasisBgLightAndTextPrimary_RendersExpectedOutput()
    {
        // Arrange
        var tagHelper = new FDCPPageHeadingTagHelper
        {
            Title = "Test Title",
            Description = "Test Description",
            Size = GCFoundation.Components.Enums.PageHeadingSize.compact,
            BackgroundColour = GCFoundation.Components.Enums.BackgroundColour.light,
            TextColour = GCFoundation.Components.Enums.TextColour.primary,
            TextEmphasis = true
        };

        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );

        var output = new TagHelperOutput("fdcp-page-heading",
            new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.Equal("fdcp-page-heading-container fdcp-page-heading-compact", output.Attributes["class"].Value);
        Assert.Contains("<article class='text-container sm:py-350 py-200 sm:pe-750 pe-450 bg-light text-primary'>", output.Content.GetContent());
        Assert.Contains("<gcds-heading tag='h1' heading-role='primary'>Test Title</gcds-heading>", output.Content.GetContent());
        Assert.Contains("<gcds-text text-role='primary'>Test Description</gcds-text>", output.Content.GetContent());
    }

    [Fact]
    public void Process_SizeLargeWithTitleAndDescription_RendersExpectedOutput()
    {
        // Arrange
        var tagHelper = new FDCPPageHeadingTagHelper
        {
            Title = "Test Title",
            Description = "Test Description",
            Size = GCFoundation.Components.Enums.PageHeadingSize.large
        };

        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );

        var output = new TagHelperOutput("fdcp-page-heading",
            new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.Equal("fdcp-page-heading-container fdcp-page-heading-large", output.Attributes["class"].Value);
        Assert.Contains("<gcds-heading tag='h1' heading-role='light'>Test Title</gcds-heading>", output.Content.GetContent());
        Assert.Contains("<gcds-text text-role='light'>Test Description</gcds-text>", output.Content.GetContent());
    }

    [Fact]
    public void Process_SizeLargeWithTitleDescriptionAndTextEmphasis_RendersExpectedOutput()
    {
        // Arrange
        var tagHelper = new FDCPPageHeadingTagHelper
        {
            Title = "Test Title",
            Description = "Test Description",
            Size = GCFoundation.Components.Enums.PageHeadingSize.large,
            TextEmphasis = true
        };

        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );

        var output = new TagHelperOutput("fdcp-page-heading",
            new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.Equal("fdcp-page-heading-container fdcp-page-heading-large", output.Attributes["class"].Value);
        Assert.Contains("<gcds-heading tag='h1' heading-role='light'>Test Title</gcds-heading>", output.Content.GetContent());
        Assert.Contains("<article class='text-container sm:py-750 py-450 sm:pe-750 pe-450 bg-primary text-light'>", output.Content.GetContent());
        Assert.Contains("<gcds-text text-role='light'>Test Description</gcds-text>", output.Content.GetContent());
    }
}