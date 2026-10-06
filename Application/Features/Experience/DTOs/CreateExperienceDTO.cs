using Domain.Enums;

namespace Application.Features.Experience.DTOs;

public sealed record CreateExperienceDTO(
    string CompanyName,
    string Position,
    ExperienceType Type,
    string Description,
    DateOnly StartDate,
    DateOnly? EndDate);