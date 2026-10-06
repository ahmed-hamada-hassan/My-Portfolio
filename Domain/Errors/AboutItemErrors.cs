using Domain.Shared;

namespace Domain.Errors;

public class AboutItemErrors
{
    public static readonly Error AboutItemNotFound = new("AboutItem.NotFound", "The requested about item was not found.");
    public static readonly Error InvalidTitle = new("AboutItem.InvalidTitle", "The title provided for the about item is invalid.");
    public static readonly Error InvalidDescription = new("AboutItem.InvalidDescription", "The description provided for the about item is invalid.");
    public static readonly Error InvalidDisplayOrder = new("AboutItem.InvalidDisplayOrder", "Display order must be between 1 and 10.");
    public static readonly Error TitleAlreadyExists = new("AboutItem.TitleAlreadyExists", "An about item with the same title already exists.");
    public static readonly Error DisplayOrderAlreadyExists = new("AboutItem.DisplayOrderAlreadyExists", "An about item with the same display order already exists.");
}
