using Domain.Enums;

namespace Application.Features.Experience.DTOs;

public sealed record GetExperienceListItemDTO(
    Guid ExperienceId,
    string CompanyName,
    string Position,
    ExperienceType Type,
    DateOnly StartDate,
    DateOnly? EndDate);
