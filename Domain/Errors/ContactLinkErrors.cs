using Domain.Shared;

namespace Domain.Errors;

public class ContactLinkErrors
{
    public static readonly Error InvalidContact = new Error("ContactLink.InvalidContact", "Contact cannot be empty.");
    public static readonly Error InvalidName = new Error("ContactLink.InvalidName", "Name cannot be empty.");
    public static readonly Error InvalidDisplayOrder = new Error("ContactLink.InvalidDisplayOrder", "Display order cannot exceed 10 or be less than 1.");
    public static readonly Error NameAlreadyExists = new Error("ContactLink.NameAlreadyExists", "A social link with the same name already exists.");
    public static readonly Error URLAlreadyExists = new Error("ContactLink.URLAlreadyExists", "A social link with the same URL already exists.");
    public static readonly Error SocialLinkNotFound = new Error("ContactLink.SocialLinkNotFound", "The social link was not found.");
    public static readonly Error DisplayOrderAlreadyExists = new Error("ContactLink.DisplayOrderAlreadyExists", "A social link with the same display order already exists.");
}
