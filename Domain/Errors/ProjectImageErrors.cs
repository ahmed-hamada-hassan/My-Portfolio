using Domain.Shared;

namespace Domain.Errors;

public class ProjectImageErrors
{
    public static readonly Error InvalidURL = new Error("ProjectImage.InvalidURL", "URL cannot be empty.");
    public static readonly Error InvalidAltText = new Error("ProjectImage.InvalidAltText", "Alt text cannot be empty.");
    public static readonly Error InvalidDisplayOrder = new Error("ProjectImage.InvalidDisplayOrder", "Display order cannot exceed 8.");
    public static readonly Error InvalidPublicId = new Error("ProjectImage.InvalidPublicId", "Public ID cannot be empty.");
    public static readonly Error MaxImagesExceeded = new Error("ProjectImage.MaxImagesExceeded", "Cannot add more than 8 images to a project.");
    public static readonly Error ImageNotFound = new Error("ProjectImage.ImageNotFound", "The specified image was not found.");
}
