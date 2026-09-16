using Domain.Enums;
using Domain.Errors;
using Domain.Shared;

namespace Domain.Entities;

public class SocialLink
{
    public Guid Id { get; private set; }
    public SocialName Name { get; private set; }
    public string URL { get; private set; } = null!;
    public string Icon { get; private set; } = null!;
    public byte DisplayOrder { get; private set; }
    public bool IsVisible { get; private set; }

    protected SocialLink() { }

    private SocialLink(SocialName name, string url, string icon, byte displayOrder, bool isVisible)
    {
        Id = Guid.NewGuid();
        Name = name;
        URL = url;
        Icon = icon;
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
    }

    public static Result<SocialLink> Create(SocialName name, string url, string icon, byte displayOrder, bool isVisible)
    {
        if (!Enum.IsDefined(typeof(SocialName), name))
            return Result<SocialLink>.Failure(SocialLinkErrors.InvalidSocialName);

        if (string.IsNullOrWhiteSpace(url))
            return Result<SocialLink>.Failure(SocialLinkErrors.InvalidURL);

        if (string.IsNullOrWhiteSpace(icon))
            return Result<SocialLink>.Failure(SocialLinkErrors.InvalidIcon);

        if (displayOrder > 8)
            return Result<SocialLink>.Failure(SocialLinkErrors.InvalidDisplayOrder);

        var socialLink = new SocialLink(name, url, icon, displayOrder, isVisible);

        return Result<SocialLink>.Success(socialLink);
    }

    public Result Update(SocialName? name, string? url, string? icon, byte? displayOrder, bool? isVisible)
    {
        var newName = name ?? Name;
        var newIcon = string.IsNullOrWhiteSpace(icon) ? Icon : icon;
        var newURL = string.IsNullOrWhiteSpace(url) ? URL : url;
        var newDisplayOrder = displayOrder ?? DisplayOrder;
        var newIsVisible = isVisible ?? IsVisible;

        if (!Enum.IsDefined(typeof(SocialName), newName))
            return Result.Failure(SocialLinkErrors.InvalidSocialName);

        if(newDisplayOrder > 10)
            return Result.Failure(SocialLinkErrors.InvalidDisplayOrder);

        Name = newName;
        URL = newURL;
        Icon = newIcon;
        DisplayOrder = newDisplayOrder;
        IsVisible = newIsVisible;

        return Result.Success();
    }
}
