using Domain.Errors;
using Domain.Shared;

namespace Domain.Entities;

public class FreelanceProfile
{
    public Guid Id { get; private set; }
    public string PlatformName { get; private set; } = null!;
    public string ProfileURL { get; private set; } = null!;
    public string Icon { get; private set; } = null!; 
    public byte DisplayOrder { get; private set; }
    public bool IsVisible { get; private set; }

    protected FreelanceProfile() { }

    private FreelanceProfile(string platformName, string profileUrl, string icon, byte displayOrder, bool isVisible)
    {
        Id = Guid.NewGuid();
        PlatformName = platformName;
        ProfileURL = profileUrl;
        Icon = icon;
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
    }

    public static Result<FreelanceProfile> Create(string platformName, string profileUrl, string icon, byte displayOrder, bool isVisible)
    {
        if (string.IsNullOrWhiteSpace(platformName))
            return Result<FreelanceProfile>.Failure(FreelanceProfileErrors.InvalidPlatformName);
        if (string.IsNullOrWhiteSpace(profileUrl))
            return Result<FreelanceProfile>.Failure(FreelanceProfileErrors.InvalidProfileUrl);
        if (string.IsNullOrWhiteSpace(icon))
            return Result<FreelanceProfile>.Failure(FreelanceProfileErrors.InvalidIcon);
        if(displayOrder > 10 || displayOrder < 1)
            return Result<FreelanceProfile>.Failure(FreelanceProfileErrors.InvalidDisplayOrder);

        var freelanceProfile = new FreelanceProfile(platformName, profileUrl, icon, displayOrder, isVisible);

        return Result<FreelanceProfile>.Success(freelanceProfile);
    }

    public Result Update(string? platformName, string? profileUrl, string? icon, byte? displayOrder, bool? isVisible)
    {
        if (platformName is not null &&
            string.IsNullOrWhiteSpace(platformName))
            return Result.Failure(
                FreelanceProfileErrors.InvalidPlatformName);

        if (profileUrl is not null &&
            string.IsNullOrWhiteSpace(profileUrl))
            return Result.Failure(
                FreelanceProfileErrors.InvalidProfileUrl);

        if (icon is not null &&
            string.IsNullOrWhiteSpace(icon))
            return Result.Failure(
                FreelanceProfileErrors.InvalidIcon);

        if (displayOrder.HasValue &&
            (displayOrder.Value > 10 || displayOrder.Value < 1))
            return Result.Failure(
                FreelanceProfileErrors.InvalidDisplayOrder);

        if (platformName is not null)
            PlatformName = platformName;

        if (profileUrl is not null)
            ProfileURL = profileUrl;

        if (icon is not null)
            Icon = icon;

        if (displayOrder.HasValue)
            DisplayOrder = displayOrder.Value;

        if (isVisible.HasValue)
            IsVisible = isVisible.Value;

        return Result.Success();
    }
}
