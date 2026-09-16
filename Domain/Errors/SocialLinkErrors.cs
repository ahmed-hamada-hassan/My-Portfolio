using Domain.Shared;

namespace Domain.Errors;

public class SocialLinkErrors
{
    public static readonly Error InvalidURL = new Error("InvalidURL", "URL cannot be empty.");
    public static readonly Error InvalidIcon = new Error("InvalidIcon", "Icon cannot be empty.");
    public static readonly Error InvalidSocialName = new Error("InvalidSocialName", "Name must be a valid SocialName.");
    public static readonly Error InvalidDisplayOrder = new Error("InvalidDisplayOrder", "Display order cannot exceed 8.");
}
