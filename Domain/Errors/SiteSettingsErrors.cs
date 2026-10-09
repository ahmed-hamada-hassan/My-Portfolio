using Domain.Shared;

namespace Domain.Errors;

public class SiteSettingsErrors
{
    public static readonly Error InvalidSiteTitle = new(
        "SiteSettings.InvalidSiteTitle",
        "Site title cannot be empty.");

    public static readonly Error InvalidFullName = new(
        "SiteSettings.InvalidFullName",
        "Full name cannot be empty.");

    public static readonly Error InvalidJobTitle = new(
        "SiteSettings.InvalidJobTitle",
        "Job title cannot be empty.");

    public static readonly Error InvalidProfileImageUrl = new(
        "SiteSettings.InvalidProfileImageUrl",
        "Profile image URL cannot be empty.");

    public static readonly Error InvalidProfileImagePublicId = new(
        "SiteSettings.InvalidProfileImagePublicId",
        "Profile image public ID cannot be empty.");

    public static readonly Error InvalidCvUrl = new(
        "SiteSettings.InvalidCvUrl",
        "CV URL cannot be empty.");

    public static readonly Error InvalidCvPublicId = new(
        "SiteSettings.InvalidCvPublicId",
        "CV public ID cannot be empty.");

    public static readonly Error SiteSettingsNotFound = new(
        "SiteSettings.NotFound",
        "Site settings not found.");

    public static readonly Error ProfileImageUploadFailed = new(
        "SiteSettings.ProfileImageUploadFailed",
        "Failed to upload profile image.");

    public static readonly Error CVNotFound = new(
        "SiteSettings.CVNotFound",
        "CV not found.");
}
