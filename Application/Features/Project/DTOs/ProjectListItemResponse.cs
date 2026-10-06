using Application.Features.Skill.DTO;

namespace Application.Features.Project.DTOs;

public record ProjectListItemResponse(
    Guid ProjectId, 
    string Title,
    string Slug,
    string Description, 
    DateOnly StartDate, 
    DateOnly? EndDate, 
    string? LiveURL, 
    string? GitHubURL, 
    string CoverImageURL, 
    List<SkillResponseDTO> Skills);
