using Domain.Errors;
using Domain.Shared;

namespace Domain.Entities;

public class AboutItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public byte DisplayOrder { get; private set; }
    public bool IsVisible { get; private set; }

    protected AboutItem() { }

    private AboutItem(string title, string description, byte displayOrder, bool isVisible)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        DisplayOrder = displayOrder;
        IsVisible = isVisible;
    }

    public static Result<AboutItem> Create(string title, string description, byte displayOrder, bool isVisible)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result<AboutItem>.Failure(AboutItemErrors.InvalidTitle);
        if (string.IsNullOrWhiteSpace(description))
            return Result<AboutItem>.Failure(AboutItemErrors.InvalidDescription);
        if (displayOrder > 10 || displayOrder < 1)
            return Result<AboutItem>.Failure(AboutItemErrors.InvalidDisplayOrder);

        var aboutItem = new AboutItem(title, description, displayOrder, isVisible);

        return Result<AboutItem>.Success(aboutItem);
    }

    public Result Update(string? title, string? description, byte? displayOrder, bool? isVisible)
    {
        if (!string.IsNullOrWhiteSpace(title))
            Title = title;

        if (!string.IsNullOrWhiteSpace(description))
            Description = description;

        if (displayOrder.HasValue && (displayOrder.Value != 0 && displayOrder.Value <= 10))
            DisplayOrder = displayOrder.Value;

        if (isVisible.HasValue)
            IsVisible = isVisible.Value;

        return Result.Success();
    }
}