using Domain.Shared;

namespace Domain.Errors;

public class SkillErrors
{
    public static readonly Error InvalidName = new Error("InvalidName", "Name cannot be empty.");
    public static readonly Error InvalidType = new Error("InvalidType", "Type must be a valid SkillType.");
    public static readonly Error InvalidIcon = new Error("InvalidIcon", "Icon cannot be empty.");
}
