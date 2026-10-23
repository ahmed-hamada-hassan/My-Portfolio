using Application.Features.Skill.Command;
using Application.Features.Skill.DTO;
using Domain.Constants;
using Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Skill.Validators_Tests;

public class UpdateSkillValidatorTests
{
    private readonly UpdateSkillValidator _validator;

    public UpdateSkillValidatorTests()
    {
        _validator = new UpdateSkillValidator();
    }

    [Fact]
    public void UpdateSkillValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var request = new UpdateSkillDTO("TypeScript", SkillType.Language, "ts.svg");
        var command = new UpdateSkillCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateSkillValidator_WithPartialFields_SuccessfullyValidates()
    {
        // Arrange
        var request = new UpdateSkillDTO("TypeScript", null, null);
        var command = new UpdateSkillCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateSkillValidator_WithEmptyId_FailsValidation()
    {
        // Arrange
        var request = new UpdateSkillDTO("TypeScript", null, null);
        var command = new UpdateSkillCommand(Guid.Empty, request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.SkillId)
            .WithErrorMessage("SkillId is required.");
    }

    [Fact]
    public void UpdateSkillValidator_WithAllNullFields_FailsValidation()
    {
        // Arrange
        var request = new UpdateSkillDTO(null, null, null);
        var command = new UpdateSkillCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateSkillRequest)
            .WithErrorMessage("At least one field must be provided.");
    }

    [Fact]
    public void UpdateSkillValidator_WithNameExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longName = new string('A', MaxLength.SkillName + 1);
        var request = new UpdateSkillDTO(longName, null, null);
        var command = new UpdateSkillCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateSkillRequest.Name)
            .WithErrorMessage($"Skill name must not exceed {MaxLength.SkillName} characters.");
    }

    [Fact]
    public void UpdateSkillValidator_WithIconExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longIcon = new string('A', MaxLength.SkillIcon + 1);
        var request = new UpdateSkillDTO(null, null, longIcon);
        var command = new UpdateSkillCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateSkillRequest.Icon)
            .WithErrorMessage($"Skill icon must not exceed {MaxLength.SkillIcon} characters.");
    }

    [Theory]
    [InlineData((SkillType)0)]
    [InlineData((SkillType)99)]
    public void UpdateSkillValidator_WithInvalidType_FailsValidation(SkillType invalidType)
    {
        // Arrange
        var request = new UpdateSkillDTO(null, invalidType, null);
        var command = new UpdateSkillCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateSkillRequest.Type)
            .WithErrorMessage("Invalid skill type.");
    }
}
