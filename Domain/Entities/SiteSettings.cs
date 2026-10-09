using Domain.Errors;
using Domain.Shared;

namespace Domain.Entities;

public class SiteSettings
{
    public Guid Id { get; private set; }

    public string SiteTitle { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string JobTitle { get; private set; } = null!;

    public string? ProfileImageURL { get; private set; }
    public string? ProfileImagePublicId { get; private set; }

    public string? CVURL { get; private set; }

    public bool AvailableForWork { get; private set; }

    protected SiteSettings() { }

    private SiteSettings(
        string siteTitle,
        string fullName,
        string jobTitle,
        string? profileImageUrl,
        string? profileImagePublicId,
        string? cvUrl,
        bool availableForWork)
    {
        Id = Guid.NewGuid();

        SiteTitle = siteTitle;
        FullName = fullName;
        JobTitle = jobTitle;

        ProfileImageURL = profileImageUrl;
        ProfileImagePublicId = profileImagePublicId;

        CVURL = cvUrl;

        AvailableForWork = availableForWork;
    }

    public static Result<SiteSettings> Create(
        string siteTitle,
        string fullName,
        string jobTitle,
        string? profileImageUrl,
        string? profileImagePublicId,
        string? cvUrl,
        bool availableForWork)
    {
        if (string.IsNullOrWhiteSpace(siteTitle))
            return Result<SiteSettings>.Failure(
                SiteSettingsErrors.InvalidSiteTitle);

        if (string.IsNullOrWhiteSpace(fullName))
            return Result<SiteSettings>.Failure(
                SiteSettingsErrors.InvalidFullName);

        if (string.IsNullOrWhiteSpace(jobTitle))
            return Result<SiteSettings>.Failure(
                SiteSettingsErrors.InvalidJobTitle);

        if (profileImageUrl is not null &&
            string.IsNullOrWhiteSpace(profileImageUrl))
            return Result<SiteSettings>.Failure(
                SiteSettingsErrors.InvalidProfileImageUrl);

        if (profileImagePublicId is not null &&
            string.IsNullOrWhiteSpace(profileImagePublicId))
            return Result<SiteSettings>.Failure(
                SiteSettingsErrors.InvalidProfileImagePublicId);

        if (cvUrl is not null &&
            string.IsNullOrWhiteSpace(cvUrl))
            return Result<SiteSettings>.Failure(
                SiteSettingsErrors.InvalidCvUrl);

        var siteSettings = new SiteSettings(
            siteTitle,
            fullName,
            jobTitle,
            profileImageUrl,
            profileImagePublicId,
            cvUrl,
            availableForWork);

        return Result<SiteSettings>.Success(siteSettings);
    }

    public Result Update(
        string? siteTitle,
        string? fullName,
        string? jobTitle,
        bool? availableForWork)
    {
        if (siteTitle is not null &&
            string.IsNullOrWhiteSpace(siteTitle))
            return Result.Failure(
                SiteSettingsErrors.InvalidSiteTitle);

        if (fullName is not null &&
            string.IsNullOrWhiteSpace(fullName))
            return Result.Failure(
                SiteSettingsErrors.InvalidFullName);

        if (jobTitle is not null &&
            string.IsNullOrWhiteSpace(jobTitle))
            return Result.Failure(
                SiteSettingsErrors.InvalidJobTitle);

        if (siteTitle is not null)
            SiteTitle = siteTitle;

        if (fullName is not null)
            FullName = fullName;

        if (jobTitle is not null)
            JobTitle = jobTitle;

        if (availableForWork.HasValue)
            AvailableForWork = availableForWork.Value;

        return Result.Success();
    }

    public Result UpdateProfileImage(
        string profileImageUrl,
        string profileImagePublicId)
    {
        if (string.IsNullOrWhiteSpace(profileImageUrl))
            return Result.Failure(
                SiteSettingsErrors.InvalidProfileImageUrl);

        if (string.IsNullOrWhiteSpace(profileImagePublicId))
            return Result.Failure(
                SiteSettingsErrors.InvalidProfileImagePublicId);

        ProfileImageURL = profileImageUrl;
        ProfileImagePublicId = profileImagePublicId;

        return Result.Success();
    }

    public Result UpdateCv(string cvUrl)
    {
        if (string.IsNullOrWhiteSpace(cvUrl))
            return Result.Failure(
                SiteSettingsErrors.InvalidCvUrl);

        CVURL = cvUrl;

        return Result.Success();
    }
}