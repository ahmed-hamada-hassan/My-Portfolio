using Domain.Entities;
using Domain.Errors;

namespace Domain.Tests;

public class SiteSettingsTests
{
    [Fact]
    public void Create_WithValidData_AssignsAllPropertiesAndGeneratedId()
    {
        var result = SiteSettings.Create("Portfolio", "Ada Lovelace", "Engineer", "profile.jpg", "profile-id", "cv.pdf", true);

        Assert.True(result.IsSuccess);
        var settings = result.Value!;
        Assert.NotEqual(Guid.Empty, settings.Id);
        Assert.Equal("Portfolio", settings.SiteTitle);
        Assert.Equal("Ada Lovelace", settings.FullName);
        Assert.Equal("Engineer", settings.JobTitle);
        Assert.Equal("profile.jpg", settings.ProfileImageURL);
        Assert.Equal("profile-id", settings.ProfileImagePublicId);
        Assert.Equal("cv.pdf", settings.CVURL);
        Assert.True(settings.AvailableForWork);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidRequiredValues_ReturnsMatchingError(string? value)
    {
        Assert.Equal(SiteSettingsErrors.InvalidSiteTitle, SiteSettings.Create(value!, "Name", "Title", null, null, null, false).Error);
        Assert.Equal(SiteSettingsErrors.InvalidFullName, SiteSettings.Create("Site", value!, "Title", null, null, null, false).Error);
        Assert.Equal(SiteSettingsErrors.InvalidJobTitle, SiteSettings.Create("Site", "Name", value!, null, null, null, false).Error);
    }

    [Fact]
    public void Update_WhenAProvidedRequiredValueIsInvalid_ReturnsErrorWithoutPartialMutation()
    {
        var settings = CreateValidSettings();

        var result = settings.Update("Updated", "   ", null, false);

        Assert.True(result.IsFailure);
        Assert.Equal(SiteSettingsErrors.InvalidFullName, result.Error);
        Assert.Equal("Portfolio", settings.SiteTitle);
        Assert.Equal("Ada Lovelace", settings.FullName);
        Assert.True(settings.AvailableForWork);
    }

    [Fact]
    public void Update_WithOnlyBooleanFalse_UpdatesBooleanAndPreservesStrings()
    {
        var settings = CreateValidSettings();

        var result = settings.Update(null, null, null, false);

        Assert.True(result.IsSuccess);
        Assert.False(settings.AvailableForWork);
        Assert.Equal("Portfolio", settings.SiteTitle);
    }

    [Fact]
    public void UpdateProfileImage_WithInvalidPublicId_DoesNotUpdateUrl()
    {
        var settings = CreateValidSettings();

        var result = settings.UpdateProfileImage("new-profile.jpg", " ");

        Assert.True(result.IsFailure);
        Assert.Equal(SiteSettingsErrors.InvalidProfileImagePublicId, result.Error);
        Assert.Equal("profile.jpg", settings.ProfileImageURL);
        Assert.Equal("profile-id", settings.ProfileImagePublicId);
    }

    private static SiteSettings CreateValidSettings() =>
        SiteSettings.Create("Portfolio", "Ada Lovelace", "Engineer", "profile.jpg", "profile-id", "cv.pdf", true).Value!;
}
