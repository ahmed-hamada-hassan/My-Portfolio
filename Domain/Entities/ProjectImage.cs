using Domain.Errors;
using Domain.Shared;

namespace Domain.Entities;

public class ProjectImage
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string URL { get; private set; } = null!;
    public string PublicId { get; private set; } = null!;
    public string AltText { get; private set; } = null!;
    public byte DisplayOrder { get; private set; }
    public bool IsCover { get; private set; }

    //Navigational Properties
    public Project Project { get; private set; } = null!;

    protected ProjectImage() { }

    private ProjectImage(Guid projectId, string url, string publicId, string altText, byte displayOrder, bool isCover)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        URL = url;
        PublicId = publicId;
        AltText = altText;
        DisplayOrder = displayOrder;
        IsCover = isCover;
    }

    public static Result<ProjectImage> Create(Guid projectId, string url, string publicId, string altText, byte displayOrder, bool isCover)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Result<ProjectImage>.Failure(ProjectImageErrors.InvalidURL);

        if(string.IsNullOrWhiteSpace(publicId))
            return Result<ProjectImage>.Failure(ProjectImageErrors.InvalidPublicId);

        if (string.IsNullOrWhiteSpace(altText))
            return Result<ProjectImage>.Failure(ProjectImageErrors.InvalidAltText);

        if(displayOrder > 8 || displayOrder < 1)
            return Result<ProjectImage>.Failure(ProjectImageErrors.InvalidDisplayOrder);

        var projectImage = new ProjectImage(projectId, url, publicId, altText, displayOrder, isCover);

        return Result<ProjectImage>.Success(projectImage);
    }

    public Result Update(string? url, string publicId, string? altText, byte? displayOrder, bool? isCover)
    {
        if (displayOrder.HasValue)
        {
            if (displayOrder.Value > 8 || displayOrder.Value < 1)
                return Result.Failure(ProjectImageErrors.InvalidDisplayOrder);
            DisplayOrder = displayOrder.Value;
        }

        if (!string.IsNullOrWhiteSpace(url))
            URL = url;

        if (!string.IsNullOrWhiteSpace(publicId))
            PublicId = publicId;

        if (!string.IsNullOrWhiteSpace(altText))
            AltText = altText;

        if (isCover.HasValue)
            IsCover = isCover.Value;

        return Result.Success();
    }


    public Result Update(string? altText, byte? displayOrder, bool? isCover)
    {
        if (displayOrder.HasValue)
        {
            if (displayOrder.Value > 8 || displayOrder.Value < 1)
                return Result.Failure(ProjectImageErrors.InvalidDisplayOrder);
            DisplayOrder = displayOrder.Value;
        }
        
        if (!string.IsNullOrWhiteSpace(altText))
            AltText = altText;

        if (isCover.HasValue)
            IsCover = isCover.Value;

        return Result.Success();
    }
}
