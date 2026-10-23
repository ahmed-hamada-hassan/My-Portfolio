using Application.Features.Experience.Command;
using Application.Features.Experience.DTOs;
using Domain.Constants;
using Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Experience.Validators_Tests;

public class UpdateExperienceValidatorTests
{
    private readonly UpdateExperienceValidator _validator;

    public UpdateExperienceValidatorTests()
    {
        _validator = new UpdateExperienceValidator();
    }

    [Fact]
    public void UpdateExperienceValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var command = new UpdateExperienceCommand(
            Guid.NewGuid(),
            new UpdateExperienceDTO(
                CompanyName: "Updated Corp",
                Position: "Senior Engineer",
                Type: ExperienceType.FullTime,
                Description: "Updated description.",
                StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
                EndDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
            )
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateExperienceValidator_WithEmptyExperienceId_FailsValidation()
    {
        // Arrange
        var command = new UpdateExperienceCommand(
            Guid.Empty,
            new UpdateExperienceDTO(null, null, null, null, null, null)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.experienceId);
    }

    [Fact]
    public void UpdateExperienceValidator_WithAllNullFieldsExceptId_SuccessfullyValidates()
    {
        // Arrange
        var command = new UpdateExperienceCommand(
            Guid.NewGuid(),
            new UpdateExperienceDTO(null, null, null, null, null, null)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateExperienceValidator_WithCompanyNameExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longName = new string('A', MaxLength.ExperienceCompanyName + 1);
        var command = new UpdateExperienceCommand(
            Guid.NewGuid(),
            new UpdateExperienceDTO(longName, null, null, null, null, null)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.updateExperienceRequest.CompanyName)
            .WithErrorMessage($"Company name must not exceed {MaxLength.ExperienceCompanyName} characters.");
    }

    [Fact]
    public void UpdateExperienceValidator_WithPositionExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longPosition = new string('A', MaxLength.ExperiencePosition + 1);
        var command = new UpdateExperienceCommand(
            Guid.NewGuid(),
            new UpdateExperienceDTO(null, longPosition, null, null, null, null)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.updateExperienceRequest.Position)
            .WithErrorMessage($"Position must not exceed {MaxLength.ExperiencePosition} characters.");
    }

    [Fact]
    public void UpdateExperienceValidator_WithDescriptionExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longDescription = new string('A', MaxLength.ExperienceDescription + 1);
        var command = new UpdateExperienceCommand(
            Guid.NewGuid(),
            new UpdateExperienceDTO(null, null, null, longDescription, null, null)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.updateExperienceRequest.Description)
            .WithErrorMessage($"Description must not exceed {MaxLength.ExperienceDescription} characters.");
    }

    [Fact]
    public void UpdateExperienceValidator_WhenStartDateIsAfterEndDate_FailsValidation()
    {
        // Arrange
        var command = new UpdateExperienceCommand(
            Guid.NewGuid(),
            new UpdateExperienceDTO(
                null,
                null,
                null,
                null,
                StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
                EndDate: DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-5))
            )
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.updateExperienceRequest.StartDate)
            .WithErrorMessage("Start date must be less than or equal to end date.");
    }

    [Fact]
    public void UpdateExperienceValidator_WhenEndDateIsInFuture_FailsValidation()
    {
        // Arrange
        var command = new UpdateExperienceCommand(
            Guid.NewGuid(),
            new UpdateExperienceDTO(
                null,
                null,
                null,
                null,
                StartDate: null,
                EndDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))
            )
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.updateExperienceRequest.EndDate)
            .WithErrorMessage("End date must be less than or equal to the current date.");
    }
}
