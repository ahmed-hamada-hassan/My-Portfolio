namespace Application.Features.Project.DTOs;

public record UpdateProjectDTO(string? Title, string? Description, DateOnly? StartDate, DateOnly? EndDate, string? LiveURL, string? GitHubURL);