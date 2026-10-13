using Domain.Entities;
using Domain.Enums;

namespace Domain.Tests;

public class ProjectSkillTests
{
    [Fact]
    public void Create_WithValidData_ReturnsProjectSkillInstance()
    {
        // Arrange
        var project = CreateValidProject();
        var skill = CreateValidSkill();

        // Act
        var projectSkill = ProjectSkill.Create(project.Id, skill.Id);

        // Assert
        Assert.NotNull(projectSkill);

        Assert.Equal(project.Id, projectSkill.ProjectId);
        Assert.Equal(skill.Id, projectSkill.SkillId);
    }

    private static Project CreateValidProject()
    {
        var result = Project.Create(
            "Original Title",
            "Original Description",
            new DateOnly(2023, 1, 1),
            new DateOnly(2023, 12, 31),
            "https://example.com",
            "https://github.com/example/project");

        Assert.True(result.IsSuccess);

        return result.Value!;
    }
    private static Skill CreateValidSkill()
    {
        var result = Skill.Create("Programming", SkillType.Framework, "programming-icon.png");

        Assert.True(result.IsSuccess);

        return result.Value!;
    }
}