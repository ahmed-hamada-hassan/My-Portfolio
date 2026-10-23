using Application.Features.Skill.Command;
using Application.Features.Skill.DTO;
using Domain.Constants;
using Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Skill.Validators_Tests;

public class CreateSkillValidatorTests
{
    private readonly CreateSkillValidator _validator;

    public CreateSkillValidatorTests()
    {
        _validator = new CreateSkillValidator();
    }

    [Fact]
    public void CreateSkillValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var request = new CreateSkillDTO(
            Name: "C#",
            Type: SkillType.Language,
            Icon: "https://cdn.example.com/csharp.svg"
        );
        var command = new CreateSkillCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateSkillValidator_WithAllNullFields_FailsValidation()
    {
        // Arrange
        var request = new CreateSkillDTO(
            Name: null!,
            Type: (SkillType)0,
            Icon: null!
        );
        var command = new CreateSkillCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateSkillRequest.Name)
            .WithErrorMessage("Skill name is required.");
        result.ShouldHaveValidationErrorFor(x => x.CreateSkillRequest.Type)
            .WithErrorMessage("Invalid skill type.");
        result.ShouldHaveValidationErrorFor(x => x.CreateSkillRequest.Icon)
            .WithErrorMessage("Skill icon is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateSkillValidator_WithEmptyName_FailsValidation(string? name)
    {
        // Arrange
        var request = new CreateSkillDTO(
            Name: name!,
            Type: SkillType.Language,
            Icon: "https://cdn.example.com/csharp.svg"
        );
        var command = new CreateSkillCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateSkillRequest.Name)
            .WithErrorMessage("Skill name is required.");
    }

    [Fact]
    public void CreateSkillValidator_WithNameExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longName = new string('A', MaxLength.SkillName + 1);
        var request = new CreateSkillDTO(
            Name: longName,
            Type: SkillType.Language,
            Icon: "https://cdn.example.com/csharp.svg"
        );
        var command = new CreateSkillCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateSkillRequest.Name)
            .WithErrorMessage($"Skill name must not exceed {MaxLength.SkillName} characters.");
    }

    [Theory]
    [InlineData((SkillType)0)]
    [InlineData((SkillType)99)]
    public void CreateSkillValidator_WithInvalidType_FailsValidation(SkillType invalidType)
    {
        // Arrange
        var request = new CreateSkillDTO(
            Name: "C#",
            Type: invalidType,
            Icon: "https://cdn.example.com/csharp.svg"
        );
        var command = new CreateSkillCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateSkillRequest.Type)
            .WithErrorMessage("Invalid skill type.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateSkillValidator_WithEmptyIcon_FailsValidation(string? icon)
    {
        // Arrange
        var request = new CreateSkillDTO(
            Name: "C#",
            Type: SkillType.Language,
            Icon: icon!
        );
        var command = new CreateSkillCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateSkillRequest.Icon)
            .WithErrorMessage("Skill icon is required.");
    }

    [Fact]
    public void CreateSkillValidator_WithIconExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longIcon = new string('A', MaxLength.SkillIcon + 1);
        var request = new CreateSkillDTO(
            Name: "C#",
            Type: SkillType.Language,
            Icon: longIcon
        );
        var command = new CreateSkillCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateSkillRequest.Icon)
            .WithErrorMessage($"Skill icon must not exceed {MaxLength.SkillIcon} characters.");
    }
}
