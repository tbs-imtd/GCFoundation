using GCFoundation.Components.Models;
namespace GCFoundation.Tests.Components.Tests.Models;
public class UserLoginViewModelTests
{
    [Fact]
    public void DisplayName_WhenFirstNameAndLastNameAreSet_ReturnsFullName()
    {
        var model = new UserLoginViewModel { FirstName = "John", LastName = "Doe" };
        Assert.Equal("John Doe", model.DisplayName);
    }
    
    [Fact]
    public void DisplayName_WhenOnlyFirstNameSet_ReturnsUserName()
    {
        var model = new UserLoginViewModel { FirstName = "John", UserName = "johndoe" };
        Assert.Equal("johndoe", model.DisplayName);
    }

    [Fact]
    public void DisplayName_WhenOnlyLastNameSet_ReturnsUserName()
    {
        var model = new UserLoginViewModel { LastName = "Doe", UserName = "johndoe" };
        Assert.Equal("johndoe", model.DisplayName);
    }

    [Fact]
    public void DisplayName_WhenFirstNameAndLastNameNotSet_ReturnsUserName()
    {
        var model = new UserLoginViewModel { UserName = "johndoe" };
        Assert.Equal("johndoe", model.DisplayName);
    }

    [Fact]
    public void DisplayName_WhenNothingSet_ReturnsEmptyString()
    {
        var model = new UserLoginViewModel();
        Assert.Equal(string.Empty, model.DisplayName);
    }

    [Fact]
    public void AccountOwnerLabel_WhenClaimsAreMissing_ReturnsSafeFallback()
    {
        var model = new UserLoginViewModel();

        Assert.Equal("U", model.AccountOwnerLabel);
    }

    [Fact]
    public void AccountOwnerLabel_WhenMenuButtonLabelIsConfigured_UsesConfiguredLabel()
    {
        var model = new UserLoginViewModel
        {
            Settings = new GCFoundation.Components.Settings.GCFoundationUserLoginSettings
            {
                MenuButtonLabel = "Account"
            }
        };

        Assert.Equal("Account", model.AccountOwnerLabel);
    }

    [Fact]
    public void GeneratedInitials_WhenUserInitialsSet_ReturnsUserInitials()
    {
        var model = new UserLoginViewModel { UserInitials = "JD" };
        Assert.Equal("JD", model.GeneratedInitials);
    }

    [Fact]
    public void GeneratedInitials_WhenFirstNameAndLastNameSet_ReturnsFirstLetterOfFirstNameAndLastName()
    {
        var model = new UserLoginViewModel { FirstName = "John", LastName = "Doe" };
        Assert.Equal("JD", model.GeneratedInitials);
    }

    [Fact]
    public void GeneratedInitials_WhenUserNameSet_ReturnsFirstLetterOfUserName()
    {
        var model = new UserLoginViewModel { UserName = "johndoe" };
        Assert.Equal("J", model.GeneratedInitials);
    }

    [Fact]
    public void GeneratedInitials_WhenNothingSet_ReturnsU()
    {
        var model = new UserLoginViewModel();
        Assert.Equal("U", model.GeneratedInitials);
    }

    [Fact]
    public void FormattedLoginTime_WhenLoginTimeSet_ReturnsNotNull()
    {
        var model = new UserLoginViewModel { LoginTime = DateTime.UtcNow };
        Assert.NotNull(model.FormattedLoginTime);
    }

    [Fact]
    public void FormattedLoginTime_WhenLoginTimeNotSet_ReturnsNull()
    {
        var model = new UserLoginViewModel();
        Assert.Null(model.FormattedLoginTime);
    }

    [Fact]
    public void MinutesUntilExpiry_WhenSessionExpiryNotSet_ReturnsNull()
    {
        var model = new UserLoginViewModel();
        Assert.Null(model.MinutesUntilExpiry);
    }

    [Fact]
    public void MinutesUntilExpiry_WhenSessionExpirySet_ReturnsMinutesUntilExpiry()
    {
        var model = new UserLoginViewModel { SessionExpiry = DateTime.UtcNow.AddMinutes(10) };
        Assert.NotNull(model.MinutesUntilExpiry);
    }

    [Fact]
    public void IsSessionExpiringSoon_WhenMinutesUntilExpiryLessThan5_ReturnsTrue()
    {
        var model = new UserLoginViewModel { SessionExpiry = DateTime.UtcNow.AddMinutes(4) };
        Assert.True(model.IsSessionExpiringSoon);
    }

    [Fact]
    public void IsSessionExpiringSoon_WhenMinutesUntilExpiryGreaterThan5_ReturnsFalse()
    {
        var model = new UserLoginViewModel { SessionExpiry = DateTime.UtcNow.AddMinutes(6) };
        Assert.False(model.IsSessionExpiringSoon);
    }

    [Fact]
    public void IsSessionExpiringSoon_WhenMinutesUntilExpiryNull_ReturnsFalse()
    {
        var model = new UserLoginViewModel();
        Assert.False(model.IsSessionExpiringSoon);
    }
}