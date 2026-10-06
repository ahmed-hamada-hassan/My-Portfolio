using Domain.Errors;
using Domain.Shared;

namespace Domain.Entities;

public class ContactLink
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Contact { get; private set; } = null!;
    public byte DisplayOrder { get; private set; }
    public bool IsVisible { get; private set; }

    protected ContactLink() { }

    private ContactLink(string name, string contact, byte displayOrder, bool isVisible)
    {
        Id = Guid.NewGuid();
        Name = name;
        Contact = contact;
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
    }

    public static Result<ContactLink> Create(string name, string contact, byte displayOrder, bool isVisible)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<ContactLink>.Failure(ContactLinkErrors.InvalidName);

        if (string.IsNullOrWhiteSpace(contact))
            return Result<ContactLink>.Failure(ContactLinkErrors.InvalidContact);

        if (displayOrder > 10 || displayOrder < 1)
            return Result<ContactLink>.Failure(ContactLinkErrors.InvalidDisplayOrder);

        var socialLink = new ContactLink(name, contact, displayOrder, isVisible);

        return Result<ContactLink>.Success(socialLink);
    }

    public Result Update(string? name, string? contact, byte? displayOrder, bool? isVisible)
    {
        if (name is not null &&
            string.IsNullOrWhiteSpace(name))
            return Result.Failure(
                ContactLinkErrors.InvalidName);

        if (contact is not null &&
            string.IsNullOrWhiteSpace(contact))
            return Result.Failure(
                ContactLinkErrors.InvalidContact);

        if (displayOrder.HasValue &&
            (displayOrder.Value == 0 || displayOrder.Value > 10))
            return Result.Failure(
                ContactLinkErrors.InvalidDisplayOrder);

        if (name is not null)
            Name = name;

        if (contact is not null)
            Contact = contact;

        if (displayOrder.HasValue)
            DisplayOrder = displayOrder.Value;

        if (isVisible.HasValue)
            IsVisible = isVisible.Value;

        return Result.Success();
    }
}
