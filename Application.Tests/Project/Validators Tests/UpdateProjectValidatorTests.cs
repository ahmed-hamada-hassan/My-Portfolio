using Application.Features.Project.Command;
using Application.Features.Project.DTOs;
using Domain.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Project.Validators_Tests;

public class UpdateProjectValidatorTests
{
    private readonly UpdateProjectValidator _validator;

    public UpdateProjectValidatorTests()
    {
        _validator = new UpdateProjectValidator();
    }

    [Fact]
    public void UpdateProjectValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var request = new UpdateProjectDTO(
            Title: "Updated Title",
            Description: "Updated Description",
            StartDate: new DateOnly(2025, 2, 1),
            EndDate: new DateOnly(2025, 8, 1),
            LiveURL: "https://example.com",
            GitHubURL: "https://github.com/example/repo"
        );
        var command = new UpdateProjectCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateProjectValidator_WithEmptyId_FailsValidation()
    {
        // Arrange
        var request = new UpdateProjectDTO("Title", null, null, null, null, null);
        var command = new UpdateProjectCommand(Guid.Empty, request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.ProjectId);
    }

    [Fact]
    public void UpdateProjectValidator_WithAllNullFields_FailsValidation()
    {
        // Arrange
        var request = new UpdateProjectDTO(null, null, null, null, null, null);
        var command = new UpdateProjectCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateProjectRequest)
            .WithErrorMessage("At least one field must be provided.");
    }

    [Fact]
    public void UpdateProjectValidator_WithTitleExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longTitle = new string('A', MaxLength.ProjectTitle + 1);
        var request = new UpdateProjectDTO(longTitle, null, null, null, null, null);
        var command = new UpdateProjectCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateProjectRequest.Title)
            .WithErrorMessage($"Title must not exceed {MaxLength.ProjectTitle} characters.");
    }

    [Fact]
    public void UpdateProjectValidator_WithDescriptionExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longDesc = new string('A', MaxLength.ProjectDescription + 1);
        var request = new UpdateProjectDTO(null, longDesc, null, null, null, null);
        var command = new UpdateProjectCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateProjectRequest.Description)
            .WithErrorMessage($"Description must not exceed {MaxLength.ProjectDescription} characters.");
    }

    [Fact]
    public void UpdateProjectValidator_WithGitHubURLExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longUrl = new string('A', MaxLength.ProjectGitHubURL + 1);
        var request = new UpdateProjectDTO(null, null, null, null, null, longUrl);
        var command = new UpdateProjectCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateProjectRequest.GitHubURL)
            .WithErrorMessage($"GitHub URL must not exceed {MaxLength.ProjectGitHubURL} characters.");
    }

    [Fact]
    public void UpdateProjectValidator_WithLiveURLExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longUrl = new string('A', MaxLength.ProjectLiveURL + 1);
        var request = new UpdateProjectDTO(null, null, null, null, longUrl, null);
        var command = new UpdateProjectCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateProjectRequest.LiveURL)
            .WithErrorMessage($"Live URL must not exceed {MaxLength.ProjectLiveURL} characters.");
    }
}
