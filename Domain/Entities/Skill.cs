using Domain.Enums;
using Domain.Errors;
using Domain.Shared;

namespace Domain.Entities;

public class Skill
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public SkillType Type { get; private set; }
    public string Icon { get; private set; } = null!;

    protected Skill() { }

    private Skill(string name, SkillType type, string icon)
    {
        Id = Guid.NewGuid();
        Name = name;
        Type = type;
        Icon = icon;
    }

    public static Result<Skill> Create(string name, SkillType type, string icon)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Skill>.Failure(SkillErrors.InvalidName);

        if (!Enum.IsDefined(typeof(SkillType), type))
            return Result<Skill>.Failure(SkillErrors.InvalidType);

        if (string.IsNullOrWhiteSpace(icon))
            return Result<Skill>.Failure(SkillErrors.InvalidIcon);

        var skill = new Skill(name, type, icon);

        return Result<Skill>.Success(skill);
    }

    public Result Update(string? name, SkillType? type, string? icon)
    {
        if(!string.IsNullOrWhiteSpace(name))
            Name = name;
        if(type.HasValue && Enum.IsDefined(typeof(SkillType), type.Value))
            Type = type.Value;
        if (!string.IsNullOrWhiteSpace(icon))
            Icon = icon;

        return Result.Success();
    }
}
