using GCFoundation.Components.Resources;
using GCFoundation.Components.TagHelpers.FDCP;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using Moq;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.FDCP;

public class FDCPSessionModalTagHelperTests
{
    private static IStringLocalizer<Modal> CreateLocalizerMock()
    {
        var mock = new Mock<IStringLocalizer<Modal>>();
        mock.Setup(l => l["Modal_Close"])
            .Returns(new LocalizedString("Modal_Close", "Close"));
        return mock.Object;
    }
    [Fact]
    public async Task ProcessAsync_RendersSessionModalWithRequiredAttributes()
    {
        // Arrange
        var tagHelper = new FDCPSessionModalTagHelper(CreateLocalizerMock())
        {
            ModalId = "session-modal",
            SessionTimeout = 3600,
            ReminderTime = 300,
            RefreshURL = new Uri("https://example.com/refresh"),
            LogoutURL = new Uri("https://example.com/logout")
        };

        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );

        var output = new TagHelperOutput("fdcp-session-modal",
            new TagHelperAttributeList(),
            (cache, encoder) => Task.FromResult<TagHelperContent>(
                new DefaultTagHelperContent()));

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("3600", output.Attributes["data-session-timeout"].Value.ToString());
        Assert.Equal("300", output.Attributes["data-reminder-time"].Value.ToString());
        Assert.Equal("https://example.com/refresh", output.Attributes["data-refresh"].Value.ToString());
        Assert.Equal("https://example.com/logout", output.Attributes["data-logout"].Value.ToString());
    }

    [Fact]
    public async Task ProcessAsync_WithNullOutput_ThrowsArgumentNullException()
    {
        // Arrange
        var tagHelper = new FDCPSessionModalTagHelper(CreateLocalizerMock());
        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await tagHelper.ProcessAsync(context, null!));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1800, 300)]
    [InlineData(7200, 600)]
    public async Task ProcessAsync_WithDifferentTimeouts_RendersCorrectly(int sessionTimeout, int reminderTime)
    {
        // Arrange
        var tagHelper = new FDCPSessionModalTagHelper(CreateLocalizerMock())
        {
            SessionTimeout = sessionTimeout,
            ReminderTime = reminderTime,
            RefreshURL = new Uri("https://example.com/refresh"),
            LogoutURL = new Uri("https://example.com/logout")
        };

        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );

        var output = new TagHelperOutput("fdcp-session-modal",
            new TagHelperAttributeList(),
            (cache, encoder) => Task.FromResult<TagHelperContent>(
                new DefaultTagHelperContent()));

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal(sessionTimeout.ToString(), output.Attributes["data-session-timeout"].Value.ToString());
        Assert.Equal(reminderTime.ToString(), output.Attributes["data-reminder-time"].Value.ToString());
    }
}