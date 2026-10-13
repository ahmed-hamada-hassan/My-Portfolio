using Domain.Entities;
using Domain.Enums;
using Domain.Errors;

namespace Domain.Tests;

public class SkillTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessfulSkillResult()
    {
        // Arrange
        var name = "Programming";
        var type = SkillType.Framework;
        var icon = "programming-icon.png";

        // Act
        var result = Skill.Create(name, type, icon);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotNull(result.Value);

        Assert.Equal(name, result.Value.Name);
        Assert.Equal(type, result.Value.Type);
        Assert.Equal(icon, result.Value.Icon);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidSkillName_ReturnsInvalidNameError(string? skillName)
    {
        // Arrange
        var type = SkillType.Framework;
        var icon = "programming-icon.png";
        // Act
        var result = Skill.Create(skillName!, type, icon);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(SkillErrors.InvalidName, result.Error);
    }

    [Fact]
    public void Create_WithInvalidSkillType_ReturnsInvalidTypeError()
    {
        // Arrange
        var name = "Programming";
        var icon = "programming-icon.png";
        var skillType = (SkillType)999; 

        // Act
        var result = Skill.Create(name, skillType, icon);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(SkillErrors.InvalidType, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidSkillIcon_ReturnsInvalidIconError(string? skillIcon)
    {
        // Arrange
        var name = "Programming";
        var type = SkillType.Framework;
        
        // Act
        var result = Skill.Create(name, type, skillIcon!);
        
        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(SkillErrors.InvalidIcon, result.Error);
    }

    [Fact]
    public void Update_WithValidData_UpdatesSkillSuccessfully()
    {
        // Arrange
        var skill = CreateValidSkill();

        var newName = "Advanced Programming";
        var newType = SkillType.Language;
        var newIcon = "advanced-programming-icon.png";

        // Act
        var result = skill.Update(newName, newType, newIcon);
        
        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(newName, skill.Name);
        Assert.Equal(newType, skill.Type);
        Assert.Equal(newIcon, skill.Icon);
    }

    [Fact]
    public void Update_WithValidName_UpdatesSkillNameSuccessfully()
    {
        // Arrange
        var skill = CreateValidSkill();

        var originalSkillType = skill.Type;
        var originalSkillIcon = skill.Icon;

        var newName = "Advanced Programming";
        // Act
        var result = skill.Update(newName, null, null);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(newName, skill.Name);
        Assert.Equal(originalSkillType, skill.Type);
        Assert.Equal(originalSkillIcon, skill.Icon);
    }

    [Fact]
    public void Update_WithValidType_UpdatesSkillTypeSuccessfully()
    {
        // Arrange
        var skill = CreateValidSkill();

        var originalSkillName = skill.Name;
        var originalSkillIcon = skill.Icon;

        var newType = SkillType.Language;
        
        // Act
        var result = skill.Update(null, newType, null);
        
        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(originalSkillName, skill.Name);
        Assert.Equal(newType, skill.Type);
        Assert.Equal(originalSkillIcon, skill.Icon);
    }

    [Fact]
    public void Update_WithValidIcon_UpdatesSkillIconSuccessfully()
    {
        // Arrange
        var skill = CreateValidSkill();

        var originalSkillName = skill.Name;
        var originalSkillType = skill.Type;

        var newIcon = "advanced-programming-icon.png";

        // Act
        var result = skill.Update(null, null, newIcon);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(originalSkillName, skill.Name);
        Assert.Equal(originalSkillType, skill.Type);
        Assert.Equal(newIcon, skill.Icon);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_KeepsOriginalName(string? newName)
    {
        // Arrange
        var skill = CreateValidSkill();

        var originalSkillName = skill.Name;
        var originalSkillType = skill.Type;
        var originalSkillIcon = skill.Icon;

        // Act
        var result = skill.Update(newName, null, null);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(originalSkillName, skill.Name);
        Assert.Equal(originalSkillType, skill.Type);
        Assert.Equal(originalSkillIcon, skill.Icon);
    }

    [Fact]
    public void Update_WithInvalidType_KeepsOriginalType()
    {
        // Arrange
        var skill = CreateValidSkill();
        var originalSkillName = skill.Name;
        var originalSkillType = skill.Type;
        var originalSkillIcon = skill.Icon;

        var newType = (SkillType)999;

        // Act
        var result = skill.Update(null, newType, null);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(originalSkillName, skill.Name);
        Assert.Equal(originalSkillType, skill.Type);
        Assert.Equal(originalSkillIcon, skill.Icon);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidIcon_KeepsOriginalIcon(string? newIcon)
    {
        // Arrange
        var skill = CreateValidSkill();
        var originalSkillName = skill.Name;
        var originalSkillType = skill.Type;
        var originalSkillIcon = skill.Icon;

        // Act
        var result = skill.Update(null, null, newIcon);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(originalSkillName, skill.Name);
        Assert.Equal(originalSkillType, skill.Type);
        Assert.Equal(originalSkillIcon, skill.Icon);
    }


    private static Skill CreateValidSkill()
    {
        var result = Skill.Create("Programming", SkillType.Framework, "programming-icon.png");

        Assert.True(result.IsSuccess);

        return result.Value!;
    }
}