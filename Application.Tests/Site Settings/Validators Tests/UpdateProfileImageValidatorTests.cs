using Application.Features.Site_Settings.Command;
using Application.Options;
using FakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Tests.Site_Settings.Validators_Tests;

public class UpdateProfileImageValidatorTests
{
    private readonly IOptionsSnapshot<SiteSettingsOptions> _optionsSnapshot;
    private readonly SiteSettingsOptions _siteSettingsOptions;
    private readonly UpdateProfileImageValidator _validator;

    public UpdateProfileImageValidatorTests()
    {
        _siteSettingsOptions = new SiteSettingsOptions
        {
            Images = new ImageUploadOptions
            {
                MaxFileSizeInBytes = 2 * 1024 * 1024, // 2 MB
                AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"],
                AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"]
            }
        };

        _optionsSnapshot = A.Fake<IOptionsSnapshot<SiteSettingsOptions>>();
        A.CallTo(() => _optionsSnapshot.Value).Returns(_siteSettingsOptions);

        _validator = new UpdateProfileImageValidator(_optionsSnapshot);
    }

    [Fact]
    public void UpdateProfileImageValidator_WithValidImage_SuccessfullyValidates()
    {
        // Arrange
        var fakeImage = CreateFakeImageFile();
        var command = new UpdateProfileImageCommand(fakeImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateProfileImageValidator_WithNullImage_FailsValidation()
    {
        // Arrange
        var command = new UpdateProfileImageCommand(null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.ProfileImage)
            .WithErrorMessage("Profile image is required.");
    }

    [Fact]
    public void UpdateProfileImageValidator_WithEmptyImage_FailsValidation()
    {
        // Arrange
        var emptyImage = CreateFakeImageFile(length: 0);
        var command = new UpdateProfileImageCommand(emptyImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.ProfileImage)
            .WithErrorMessage("Profile image cannot be empty.");
    }

    [Fact]
    public void UpdateProfileImageValidator_WithImageExceedingMaxFileSize_FailsValidation()
    {
        // Arrange
        var oversizedImage = CreateFakeImageFile(length: _siteSettingsOptions.Images.MaxFileSizeInBytes + 1);
        var command = new UpdateProfileImageCommand(oversizedImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.ProfileImage)
            .WithErrorMessage($"Profile image size cannot exceed {_siteSettingsOptions.Images.MaxFileSizeInBytes / 1024 / 1024} MB.");
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    [InlineData("image/gif")]
    public void UpdateProfileImageValidator_WithDisallowedContentType_FailsValidation(string contentType)
    {
        // Arrange
        var invalidImage = CreateFakeImageFile(contentType: contentType);
        var command = new UpdateProfileImageCommand(invalidImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.ProfileImage)
            .WithErrorMessage("Profile image content type is not allowed.");
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("IMAGE/JPEG")]
    [InlineData("image/png")]
    [InlineData("IMAGE/WEBP")]
    public void UpdateProfileImageValidator_WithAllowedContentTypesCaseInsensitive_SuccessfullyValidates(string contentType)
    {
        // Arrange
        var validImage = CreateFakeImageFile(contentType: contentType);
        var command = new UpdateProfileImageCommand(validImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static IFormFile CreateFakeImageFile(
        string fileName = "profile.jpg",
        long length = 1024,
        string contentType = "image/jpeg")
    {
        var file = A.Fake<IFormFile>();
        A.CallTo(() => file.FileName).Returns(fileName);
        A.CallTo(() => file.Length).Returns(length);
        A.CallTo(() => file.ContentType).Returns(contentType);
        return file;
    }
}
