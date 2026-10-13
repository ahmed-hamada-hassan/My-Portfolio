using Domain.Entities;
using Domain.Errors;

namespace Domain.Tests;

public class FreelanceProfileTests
{
    [Fact]
    public void Create_WithValidData_AssignsPropertiesAndGeneratedId()
    {
        var result = FreelanceProfile.Create("Upwork", "https://upwork.com/profile", "briefcase", 1, true);

        Assert.True(result.IsSuccess);
        var profile = result.Value!;
        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.Equal("Upwork", profile.PlatformName);
        Assert.Equal("https://upwork.com/profile", profile.ProfileURL);
        Assert.Equal("briefcase", profile.Icon);
        Assert.Equal((byte)1, profile.DisplayOrder);
        Assert.True(profile.IsVisible);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Create_WithInvalidDisplayOrder_ReturnsError(byte displayOrder)
    {
        var result = FreelanceProfile.Create("Upwork", "https://upwork.com/profile", "briefcase", displayOrder, true);

        Assert.True(result.IsFailure);
        Assert.Equal(FreelanceProfileErrors.InvalidDisplayOrder, result.Error);
    }

    [Fact]
    public void Update_WithInvalidValue_ReturnsErrorWithoutPartialMutation()
    {
        var profile = CreateValidProfile();

        var result = profile.Update("Updated", "https://updated.example", " ", 2, false);

        Assert.True(result.IsFailure);
        Assert.Equal(FreelanceProfileErrors.InvalidIcon, result.Error);
        Assert.Equal("Upwork", profile.PlatformName);
        Assert.Equal("https://upwork.com/profile", profile.ProfileURL);
        Assert.Equal("briefcase", profile.Icon);
        Assert.Equal((byte)1, profile.DisplayOrder);
        Assert.True(profile.IsVisible);
    }

    [Fact]
    public void Update_WithOnlyBooleanFalse_UpdatesBooleanAndPreservesOtherValues()
    {
        var profile = CreateValidProfile();

        var result = profile.Update(null, null, null, null, false);

        Assert.True(result.IsSuccess);
        Assert.False(profile.IsVisible);
        Assert.Equal("Upwork", profile.PlatformName);
    }

    private static FreelanceProfile CreateValidProfile() =>
        FreelanceProfile.Create("Upwork", "https://upwork.com/profile", "briefcase", 1, true).Value!;
}
