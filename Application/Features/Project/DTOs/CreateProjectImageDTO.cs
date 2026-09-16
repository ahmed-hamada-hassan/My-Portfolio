namespace Application.Features.Project.DTOs;

public record CreateProjectImageDTO(string URL, string AltText, byte DisplayOrder, bool IsCover);