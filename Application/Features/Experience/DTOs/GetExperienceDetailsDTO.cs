using Domain.Enums;

namespace Application.Features.Experience.DTOs;

public sealed record GetExperienceDetailsDTO(
    Guid ExperienceId,
    string CompanyName,
    string Position,
    string Description,
    ExperienceType Type,
    DateOnly StartDate,
    DateOnly? EndDate);