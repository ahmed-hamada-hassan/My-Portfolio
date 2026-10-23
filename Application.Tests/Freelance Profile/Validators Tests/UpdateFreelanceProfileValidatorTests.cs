using Application.Features.Freelance_Profile.Command;
using Application.Features.Freelance_Profile.DTOs;
using Domain.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Freelance_Profile.Validators_Tests;

public class UpdateFreelanceProfileValidatorTests
{
    private readonly UpdateFreelanceProfileValidator _validator;

    public UpdateFreelanceProfileValidatorTests()
    {
        _validator = new UpdateFreelanceProfileValidator();
    }

    [Fact]
    public void UpdateFreelanceProfileValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var request = new UpdateFreelanceProfileDTO(
            PlatformName: "Fiverr",
            ProfileURL: "https://www.fiverr.com/test",
            Icon: "https://cdn.example.com/fiverr.svg",
            DisplayOrder: 2,
            IsVisible: false
        );
        var command = new UpdateFreelanceProfileCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateFreelanceProfileValidator_WithPartialFields_SuccessfullyValidates()
    {
        // Arrange
        var request = new UpdateFreelanceProfileDTO("Upwork", null, null, null, null);
        var command = new UpdateFreelanceProfileCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateFreelanceProfileValidator_WithEmptyId_FailsValidation()
    {
        // Arrange
        var request = new UpdateFreelanceProfileDTO("Upwork", null, null, null, null);
        var command = new UpdateFreelanceProfileCommand(Guid.Empty, request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.FreelanceProfileId);
    }

    [Fact]
    public void UpdateFreelanceProfileValidator_WithAllNullFields_FailsValidation()
    {
        // Arrange
        var request = new UpdateFreelanceProfileDTO(null, null, null, null, null);
        var command = new UpdateFreelanceProfileCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateFreelanceProfileRequest)
            .WithErrorMessage("At least one field must be provided.");
    }

    [Fact]
    public void UpdateFreelanceProfileValidator_WithPlatformNameExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longName = new string('A', MaxLength.FreelanceProfilePlatformName + 1);
        var request = new UpdateFreelanceProfileDTO(longName, null, null, null, null);
        var command = new UpdateFreelanceProfileCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateFreelanceProfileRequest.PlatformName);
    }

    [Fact]
    public void UpdateFreelanceProfileValidator_WithProfileURLExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longUrl = "https://example.com/" + new string('A', MaxLength.FreelanceProfileProfileURL);
        var request = new UpdateFreelanceProfileDTO(null, longUrl, null, null, null);
        var command = new UpdateFreelanceProfileCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateFreelanceProfileRequest.ProfileURL);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com")]
    [InlineData("")]
    public void UpdateFreelanceProfileValidator_WithInvalidProfileURL_FailsValidation(string invalidUrl)
    {
        // Arrange
        var request = new UpdateFreelanceProfileDTO(null, invalidUrl, null, null, null);
        var command = new UpdateFreelanceProfileCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateFreelanceProfileRequest.ProfileURL)
            .WithErrorMessage("Profile URL must be a valid HTTP or HTTPS URL.");
    }

    [Fact]
    public void UpdateFreelanceProfileValidator_WithIconExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longIcon = "https://example.com/" + new string('A', MaxLength.FreelanceProfileIcon);
        var request = new UpdateFreelanceProfileDTO(null, null, longIcon, null, null);
        var command = new UpdateFreelanceProfileCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateFreelanceProfileRequest.Icon);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com")]
    [InlineData("")]
    public void UpdateFreelanceProfileValidator_WithInvalidIcon_FailsValidation(string invalidIcon)
    {
        // Arrange
        var request = new UpdateFreelanceProfileDTO(null, null, invalidIcon, null, null);
        var command = new UpdateFreelanceProfileCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateFreelanceProfileRequest.Icon)
            .WithErrorMessage("Icon must be a valid HTTP or HTTPS URL.");
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)11)]
    [InlineData((byte)255)]
    public void UpdateFreelanceProfileValidator_WithInvalidDisplayOrder_FailsValidation(byte displayOrder)
    {
        // Arrange
        var request = new UpdateFreelanceProfileDTO(null, null, null, displayOrder, null);
        var command = new UpdateFreelanceProfileCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateFreelanceProfileRequest.DisplayOrder)
            .WithErrorMessage("Display order must be between 1 and 10.");
    }
}
