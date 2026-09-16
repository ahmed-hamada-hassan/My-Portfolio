using Domain.Shared;

namespace Domain.Errors;

public class ProjectImageErrors
{
    public static readonly Error InvalidURL = new Error("InvalidURL", "URL cannot be empty.");
    public static readonly Error InvalidAltText = new Error("InvalidAltText", "Alt text cannot be empty.");
    public static readonly Error InvalidDisplayOrder = new Error("InvalidDisplayOrder", "Display order cannot exceed 8.");
}
