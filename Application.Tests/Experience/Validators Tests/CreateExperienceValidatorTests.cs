using Application.Features.Experience.Command;
using Application.Features.Experience.DTOs;
using Domain.Constants;
using Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Experience.Validators_Tests;

public class CreateExperienceValidatorTests
{
    private readonly CreateExperienceValidator _validator;

    public CreateExperienceValidatorTests()
    {
        _validator = new CreateExperienceValidator();
    }

    [Fact]
    public void CreateExperienceValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var request = new CreateExperienceDTO(
            CompanyName: "Valid Company",
            Position: "Software Engineer",
            Type: ExperienceType.FullTime,
            Description: "Valid Description",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
            EndDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        );
        var command = new CreateExperienceCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateExperienceValidator_WithOngoingExperience_SuccessfullyValidates()
    {
        // Arrange
        var request = new CreateExperienceDTO(
            CompanyName: "Valid Company",
            Position: "Software Engineer",
            Type: ExperienceType.FullTime,
            Description: "Valid Description",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            EndDate: null
        );
        var command = new CreateExperienceCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateExperienceValidator_WithAllNullFields_FailsValidation()
    {
        // Arrange
        var request = new CreateExperienceDTO(
            CompanyName: null!,
            Position: null!,
            Type: (ExperienceType)999,
            Description: null!,
            StartDate: default,
            EndDate: null
        );
        var command = new CreateExperienceCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createExperienceRequest.CompanyName)
            .WithErrorMessage("Company name is required.");
        result.ShouldHaveValidationErrorFor(x => x.createExperienceRequest.Position)
            .WithErrorMessage("Position is required.");
        result.ShouldHaveValidationErrorFor(x => x.createExperienceRequest.Type)
            .WithErrorMessage("Type must be a valid enum value.");
        result.ShouldHaveValidationErrorFor(x => x.createExperienceRequest.Description)
            .WithErrorMessage("Description is required.");
        result.ShouldHaveValidationErrorFor(x => x.createExperienceRequest.StartDate)
            .WithErrorMessage("Start date is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateExperienceValidator_WithInvalidCompanyName_FailsValidation(string? companyName)
    {
        // Arrange
        var request = new CreateExperienceDTO(
            CompanyName: companyName!,
            Position: "Developer",
            Type: ExperienceType.FullTime,
            Description: "Description",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            EndDate: null
        );
        var command = new CreateExperienceCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createExperienceRequest.CompanyName)
            .WithErrorMessage("Company name is required.");
    }

    [Fact]
    public void CreateExperienceValidator_WithCompanyNameExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longName = new string('A', MaxLength.ExperienceCompanyName + 1);
        var request = new CreateExperienceDTO(
            CompanyName: longName,
            Position: "Developer",
            Type: ExperienceType.FullTime,
            Description: "Description",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            EndDate: null
        );
        var command = new CreateExperienceCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createExperienceRequest.CompanyName)
            .WithErrorMessage($"Company name must not exceed {MaxLength.ExperienceCompanyName} characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateExperienceValidator_WithInvalidPosition_FailsValidation(string? position)
    {
        // Arrange
        var request = new CreateExperienceDTO(
            CompanyName: "Company",
            Position: position!,
            Type: ExperienceType.FullTime,
            Description: "Description",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            EndDate: null
        );
        var command = new CreateExperienceCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createExperienceRequest.Position)
            .WithErrorMessage("Position is required.");
    }

    [Fact]
    public void CreateExperienceValidator_WithPositionExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longPosition = new string('A', MaxLength.ExperiencePosition + 1);
        var request = new CreateExperienceDTO(
            CompanyName: "Company",
            Position: longPosition,
            Type: ExperienceType.FullTime,
            Description: "Description",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            EndDate: null
        );
        var command = new CreateExperienceCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createExperienceRequest.Position)
            .WithErrorMessage($"Position must not exceed {MaxLength.ExperiencePosition} characters.");
    }

    [Fact]
    public void CreateExperienceValidator_WithInvalidEnumType_FailsValidation()
    {
        // Arrange
        var request = new CreateExperienceDTO(
            CompanyName: "Company",
            Position: "Position",
            Type: (ExperienceType)999,
            Description: "Description",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            EndDate: null
        );
        var command = new CreateExperienceCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createExperienceRequest.Type)
            .WithErrorMessage("Type must be a valid enum value.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateExperienceValidator_WithInvalidDescription_FailsValidation(string? description)
    {
        // Arrange
        var request = new CreateExperienceDTO(
            CompanyName: "Company",
            Position: "Position",
            Type: ExperienceType.FullTime,
            Description: description!,
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            EndDate: null
        );
        var command = new CreateExperienceCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createExperienceRequest.Description)
            .WithErrorMessage("Description is required.");
    }

    [Fact]
    public void CreateExperienceValidator_WithDescriptionExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longDescription = new string('A', MaxLength.ExperienceDescription + 1);
        var request = new CreateExperienceDTO(
            CompanyName: "Company",
            Position: "Position",
            Type: ExperienceType.FullTime,
            Description: longDescription,
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            EndDate: null
        );
        var command = new CreateExperienceCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createExperienceRequest.Description)
            .WithErrorMessage($"Description must not exceed {MaxLength.ExperienceDescription} characters.");
    }

    [Fact]
    public void CreateExperienceValidator_WhenStartDateIsAfterEndDate_FailsValidation()
    {
        // Arrange
        var request = new CreateExperienceDTO(
            CompanyName: "Company",
            Position: "Position",
            Type: ExperienceType.FullTime,
            Description: "Description",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            EndDate: DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6))
        );
        var command = new CreateExperienceCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createExperienceRequest.StartDate)
            .WithErrorMessage("Start date must be less than or equal to end date.");
    }

    [Fact]
    public void CreateExperienceValidator_WhenEndDateIsInFuture_FailsValidation()
    {
        // Arrange
        var futureEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        var request = new CreateExperienceDTO(
            CompanyName: "Company",
            Position: "Position",
            Type: ExperienceType.FullTime,
            Description: "Description",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            EndDate: futureEndDate
        );
        var command = new CreateExperienceCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createExperienceRequest.EndDate)
            .WithErrorMessage("End date must be less than or equal to the current date.");
    }
}
