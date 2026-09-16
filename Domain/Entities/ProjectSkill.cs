namespace Domain.Entities;

public class ProjectSkill
{
    public Guid ProjectId { get; private set; }
    public Guid SkillId { get; private set; }

    protected ProjectSkill() { }

    private ProjectSkill(Guid projectId, Guid skillId)
    {
        ProjectId = projectId;
        SkillId = skillId;
    }

    public static ProjectSkill Create(Guid projectId, Guid skillId)
    {
        return new ProjectSkill(projectId, skillId);
    }
}
