using GCFoundation.Components.Helpers;
namespace GCFoundation.Tests.Components.Tests.Helpers;
public class StaticResourceHelperTests
{
    private static readonly string VersionSuffix =
        $"?v={typeof(StaticResourceHelper).Assembly.GetName().Version?.ToString() ?? "1"}";

    [Fact]
    public void ConfigureVirtualDirectoryPrefix_ShouldSetEmptyPrefix_WhenNull()
    {
        StaticResourceHelper.ConfigureVirtualDirectoryPrefix(null);
        var result = StaticResourceHelper.GetResourcePath("test.css");
        Assert.Equal($"/_content/GCFoundation.Components/test.css{VersionSuffix}", result);
    }

    [Fact]
    public void ConfigureVirtualDirectoryPrefix_ShouldSetEmptyPrefix_WhenEmpty()
    {
        StaticResourceHelper.ConfigureVirtualDirectoryPrefix(string.Empty);
        var result = StaticResourceHelper.GetResourcePath("test.css");
        Assert.Equal($"/_content/GCFoundation.Components/test.css{VersionSuffix}", result);
    }

    [Fact]
    public void ConfigureVirtualDirectoryPrefix_ShouldSetEmptyPrefix_WhenWhitespace()
    {
        StaticResourceHelper.ConfigureVirtualDirectoryPrefix(" ");
        var result = StaticResourceHelper.GetResourcePath("test.css");
        Assert.Equal($"/_content/GCFoundation.Components/test.css{VersionSuffix}", result);
    }

    [Fact]
    public void ConfigureVirtualDirectoryPrefix_ShouldSetEmptyPrefix_WhenOnlySlashes()
    {
        StaticResourceHelper.ConfigureVirtualDirectoryPrefix("/");
        var result = StaticResourceHelper.GetResourcePath("test.css");
        Assert.Equal($"/_content/GCFoundation.Components/test.css{VersionSuffix}", result);
    }

    [Fact]
    public void ConfigureVirtualDirectoryPrefix_ShouldSetPrefix_WhenValidPath()
    {
        StaticResourceHelper.ConfigureVirtualDirectoryPrefix("/test");
        var result = StaticResourceHelper.GetResourcePath("test.css");
        Assert.Equal($"/test/_content/GCFoundation.Components/test.css{VersionSuffix}", result);
    }

    [Fact]
    public void ConfigureVirtualDirectoryPrefix_ShouldTrimTrailingSlash_WhenPathHasTrailingSlash()
    {
        StaticResourceHelper.ConfigureVirtualDirectoryPrefix("/test/");
        var result = StaticResourceHelper.GetResourcePath("test.css");
        Assert.Equal($"/test/_content/GCFoundation.Components/test.css{VersionSuffix}", result);
    }

    [Fact]
    public void ConfigureVirtualDirectoryPrefix_ShouldAddLeadingSlash_WhenPathHasNoLeadingSlash()
    {
        StaticResourceHelper.ConfigureVirtualDirectoryPrefix("test");
        var result = StaticResourceHelper.GetResourcePath("test.css");
        Assert.Equal($"/test/_content/GCFoundation.Components/test.css{VersionSuffix}", result);
    }

    [Fact]
    public void ConfigureVirtualDirectoryPrefix_ShouldPreserveMiddleSlashes_WhenPathHasSubdirectories()
    {
        StaticResourceHelper.ConfigureVirtualDirectoryPrefix("/test/subdir");
        var result = StaticResourceHelper.GetResourcePath("test.css");
        Assert.Equal($"/test/subdir/_content/GCFoundation.Components/test.css{VersionSuffix}", result);
    }

}