using Domain.Enums;

namespace Application.Features.Skill.DTO;

public record SkillResponseDTO(
    Guid SkillId,
    string Name,
    SkillType Type,
    string Icon);