using Domain.Enums;

namespace Application.Features.Experience.DTOs;

public sealed record UpdateExperienceDTO(
    string? CompanyName,
    string? Position,
    ExperienceType? Type,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate);