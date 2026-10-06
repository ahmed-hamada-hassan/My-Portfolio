using Domain.Shared;

namespace Domain.Errors;

public sealed class FreelanceProfileErrors
{
    public static readonly Error InvalidPlatformName = new Error("FreelanceProfile.InvalidPlatformName", "Platform name cannot be empty.");
    public static readonly Error InvalidProfileUrl = new Error("FreelanceProfile.InvalidProfileUrl", "Profile URL cannot be empty.");
    public static readonly Error InvalidIcon = new Error("FreelanceProfile.InvalidIcon", "Icon cannot be empty.");
    public static readonly Error InvalidDisplayOrder = new Error("FreelanceProfile.InvalidDisplayOrder", "Display order must be between 1 and 10.");
    public static readonly Error PlatformNameAlreadyExists = new Error("FreelanceProfile.PlatformNameAlreadyExists", "A freelance profile with the same platform name already exists.");
    public static readonly Error ProfileUrlAlreadyExists = new Error("FreelanceProfile.ProfileUrlAlreadyExists", "A freelance profile with the same profile URL already exists.");
    public static readonly Error IconAlreadyExists = new Error("FreelanceProfile.IconAlreadyExists", "A freelance profile with the same icon already exists.");
    public static readonly Error DisplayOrderAlreadyExists = new Error("FreelanceProfile.DisplayOrderAlreadyExists", "A freelance profile with the same display order already exists.");
    public static readonly Error FreelanceProfileNotFound = new Error("FreelanceProfile.NotFound", "The freelance profile was not found.");
}
