using Application.Features.Freelance_Profile.Command;
using Application.Features.Freelance_Profile.DTOs;
using Domain.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Freelance_Profile.Validators_Tests;

public class CreateFreelanceProfileValidatorTests
{
    private readonly CreateFreelanceProfileValidator _validator;

    public CreateFreelanceProfileValidatorTests()
    {
        _validator = new CreateFreelanceProfileValidator();
    }

    [Fact]
    public void CreateFreelanceProfileValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var request = new CreateFreelanceProfileDTO(
            PlatformName: "Upwork",
            ProfileURL: "https://www.upwork.com/freelancers/~test",
            Icon: "https://cdn.example.com/icons/upwork.svg",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateFreelanceProfileValidator_WithAllNullFields_FailsValidation()
    {
        // Arrange
        var request = new CreateFreelanceProfileDTO(
            PlatformName: null!,
            ProfileURL: null!,
            Icon: null!,
            DisplayOrder: 0,
            IsVisible: false
        );
        var command = new CreateFreelanceProfileCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createFreelanceProfileRequest.PlatformName);
        result.ShouldHaveValidationErrorFor(x => x.createFreelanceProfileRequest.ProfileURL);
        result.ShouldHaveValidationErrorFor(x => x.createFreelanceProfileRequest.Icon);
        result.ShouldHaveValidationErrorFor(x => x.createFreelanceProfileRequest.DisplayOrder)
            .WithErrorMessage("Display order must be between 1 and 10.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFreelanceProfileValidator_WithEmptyPlatformName_FailsValidation(string? platformName)
    {
        // Arrange
        var request = new CreateFreelanceProfileDTO(
            PlatformName: platformName!,
            ProfileURL: "https://www.upwork.com/test",
            Icon: "https://cdn.example.com/upwork.svg",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createFreelanceProfileRequest.PlatformName);
    }

    [Fact]
    public void CreateFreelanceProfileValidator_WithPlatformNameExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longPlatform = new string('A', MaxLength.FreelanceProfilePlatformName + 1);
        var request = new CreateFreelanceProfileDTO(
            PlatformName: longPlatform,
            ProfileURL: "https://www.upwork.com/test",
            Icon: "https://cdn.example.com/upwork.svg",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createFreelanceProfileRequest.PlatformName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFreelanceProfileValidator_WithEmptyProfileURL_FailsValidation(string? profileUrl)
    {
        // Arrange
        var request = new CreateFreelanceProfileDTO(
            PlatformName: "Upwork",
            ProfileURL: profileUrl!,
            Icon: "https://cdn.example.com/upwork.svg",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createFreelanceProfileRequest.ProfileURL);
    }

    [Fact]
    public void CreateFreelanceProfileValidator_WithProfileURLExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longUrl = "https://upwork.com/" + new string('a', MaxLength.FreelanceProfileProfileURL);
        var request = new CreateFreelanceProfileDTO(
            PlatformName: "Upwork",
            ProfileURL: longUrl,
            Icon: "https://cdn.example.com/upwork.svg",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createFreelanceProfileRequest.ProfileURL);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://upwork.com")]
    [InlineData("mailto:user@upwork.com")]
    public void CreateFreelanceProfileValidator_WithInvalidProfileURLFormat_FailsValidation(string invalidUrl)
    {
        // Arrange
        var request = new CreateFreelanceProfileDTO(
            PlatformName: "Upwork",
            ProfileURL: invalidUrl,
            Icon: "https://cdn.example.com/upwork.svg",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createFreelanceProfileRequest.ProfileURL)
            .WithErrorMessage("Profile URL must be a valid HTTP or HTTPS URL.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFreelanceProfileValidator_WithEmptyIcon_FailsValidation(string? icon)
    {
        // Arrange
        var request = new CreateFreelanceProfileDTO(
            PlatformName: "Upwork",
            ProfileURL: "https://www.upwork.com/test",
            Icon: icon!,
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createFreelanceProfileRequest.Icon);
    }

    [Fact]
    public void CreateFreelanceProfileValidator_WithIconExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longIcon = "https://cdn.example.com/" + new string('a', MaxLength.FreelanceProfileIcon);
        var request = new CreateFreelanceProfileDTO(
            PlatformName: "Upwork",
            ProfileURL: "https://www.upwork.com/test",
            Icon: longIcon,
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createFreelanceProfileRequest.Icon);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://cdn.example.com/icon.svg")]
    public void CreateFreelanceProfileValidator_WithInvalidIconFormat_FailsValidation(string invalidIcon)
    {
        // Arrange
        var request = new CreateFreelanceProfileDTO(
            PlatformName: "Upwork",
            ProfileURL: "https://www.upwork.com/test",
            Icon: invalidIcon,
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createFreelanceProfileRequest.Icon)
            .WithErrorMessage("Icon must be a valid HTTP or HTTPS URL.");
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)11)]
    [InlineData((byte)255)]
    public void CreateFreelanceProfileValidator_WithInvalidDisplayOrder_FailsValidation(byte displayOrder)
    {
        // Arrange
        var request = new CreateFreelanceProfileDTO(
            PlatformName: "Upwork",
            ProfileURL: "https://www.upwork.com/test",
            Icon: "https://cdn.example.com/upwork.svg",
            DisplayOrder: displayOrder,
            IsVisible: true
        );
        var command = new CreateFreelanceProfileCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createFreelanceProfileRequest.DisplayOrder)
            .WithErrorMessage("Display order must be between 1 and 10.");
    }
}
