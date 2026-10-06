using Domain.Enums;

namespace Application.Features.Skill.DTO;

public record CreateSkillDTO(string Name, SkillType Type, string Icon);