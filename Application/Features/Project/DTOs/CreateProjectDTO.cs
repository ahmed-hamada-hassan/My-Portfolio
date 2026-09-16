namespace Application.Features.Project.DTOs;

public record CreateProjectDTO(string Title, string Description, DateOnly StartDate,
    DateOnly? EndDate, string? LiveURL, string? GitHubURL, IEnumerable<CreateProjectImageDTO> ProjectImages);