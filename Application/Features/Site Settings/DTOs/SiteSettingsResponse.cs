namespace Application.Features.Site_Settings.DTOs;

public sealed record SiteSettingsResponse(
    string SiteTitle,
    string FullName,
    string JobTitle,
    string? ProfileImageURL,
    string? CVURL,
    bool AvailableForWork);