using Domain.Shared;

namespace Domain.Errors;

public class SkillErrors
{
    public static readonly Error InvalidName = new Error("Skill.InvalidName", "Name cannot be empty.");
    public static readonly Error InvalidType = new Error("Skill.InvalidType", "Type must be a valid SkillType.");
    public static readonly Error InvalidIcon = new Error("Skill.InvalidIcon", "Icon cannot be empty.");
    public static readonly Error NotFound = new Error("Skill.NotFound", "Skill not found.");
    public static readonly Error SkillAlreadyExist = new Error("Skill.AlreadyExist", "Skill with the same name already exists.");
    public static readonly Error SkillIsUsed = new Error("Skill.IsUsed", "Skill is used in a project and cannot be deleted.");
}
