namespace Application.Features.Project.DTOs;

public record ProjectImageResponse(
    Guid ImageId,
    string URL,
    string AltText,
    byte DisplayOrder,
    bool IsCover);