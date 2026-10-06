using Application.Features.Skill.DTO;

namespace Application.Features.Project.DTOs;

public record ProjectDetailsResponse(
    Guid ProjectId,
    string Title,
    string Slug,
    string Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? LiveURL,
    string? GitHubURL,

    List<ProjectImageResponse> ProjectImages,

    List<SkillResponseDTO> Skills);
