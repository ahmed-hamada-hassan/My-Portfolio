using Domain.Enums;

namespace Application.Features.Skill.DTO;

public record UpdateSkillDTO(string? Name, SkillType? Type, string? Icon);