using Application.Features.Project_Images.DTOs;

namespace Application.Features.Project.DTOs;

public record CreateProjectDTO(
    string Title, 
    string Description, 
    DateOnly StartDate,
    DateOnly? EndDate, 
    string? LiveURL, 
    string? GitHubURL, 
    IReadOnlyList<AddProjectImageDTO> ProjectImages,
    IReadOnlyList<Guid> SkillIds);