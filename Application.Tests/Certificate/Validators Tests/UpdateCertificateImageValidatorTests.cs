using Application.Features.Certificate.Command;
using Application.Options;
using FakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Tests.Certificate.Validators_Tests;

public class UpdateCertificateImageValidatorTests
{
    private readonly IOptionsSnapshot<SiteSettingsOptions> _optionsSnapshot;
    private readonly SiteSettingsOptions _siteSettingsOptions;
    private readonly UpdateCertificateImageValidator _validator;

    public UpdateCertificateImageValidatorTests()
    {
        _siteSettingsOptions = new SiteSettingsOptions
        {
            Images = new ImageUploadOptions
            {
                MaxFileSizeInBytes = 5 * 1024 * 1024, // 5 MB
                AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"],
                AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"]
            }
        };

        _optionsSnapshot = A.Fake<IOptionsSnapshot<SiteSettingsOptions>>();
        A.CallTo(() => _optionsSnapshot.Value).Returns(_siteSettingsOptions);

        _validator = new UpdateCertificateImageValidator(_optionsSnapshot);
    }

    [Fact]
    public void UpdateCertificateImageValidator_WithValidImage_SuccessfullyValidates()
    {
        // Arrange
        var fakeImage = CreateFakeImageFile();
        var command = new UpdateCertificateImageCommand(Guid.NewGuid(), fakeImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateCertificateImageValidator_WithNullImage_FailsValidation()
    {
        // Arrange
        var command = new UpdateCertificateImageCommand(Guid.NewGuid(), null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CertificateImage)
            .WithErrorMessage("Certificate image is required.");
    }

    [Fact]
    public void UpdateCertificateImageValidator_WithEmptyImage_FailsValidation()
    {
        // Arrange
        var emptyImage = CreateFakeImageFile(length: 0);
        var command = new UpdateCertificateImageCommand(Guid.NewGuid(), emptyImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CertificateImage)
            .WithErrorMessage("Certificate image cannot be empty.");
    }

    [Fact]
    public void UpdateCertificateImageValidator_WithImageExceedingMaxFileSize_FailsValidation()
    {
        // Arrange
        var oversizedImage = CreateFakeImageFile(length: _siteSettingsOptions.Images.MaxFileSizeInBytes + 1);
        var command = new UpdateCertificateImageCommand(Guid.NewGuid(), oversizedImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CertificateImage)
            .WithErrorMessage($"Certificate image size must not exceed {_siteSettingsOptions.Images.MaxFileSizeInBytes / 1024 / 1024} MB.");
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    [InlineData("image/gif")]
    public void UpdateCertificateImageValidator_WithDisallowedContentType_FailsValidation(string contentType)
    {
        // Arrange
        var invalidImage = CreateFakeImageFile(contentType: contentType);
        var command = new UpdateCertificateImageCommand(Guid.NewGuid(), invalidImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CertificateImage)
            .WithErrorMessage("Certificate image content type is not allowed.");
    }

    [Theory]
    [InlineData("image.gif")]
    [InlineData("document.pdf")]
    [InlineData("image.bmp")]
    [InlineData("image")]
    public void UpdateCertificateImageValidator_WithDisallowedExtension_FailsValidation(string fileName)
    {
        // Arrange
        var invalidImage = CreateFakeImageFile(fileName: fileName);
        var command = new UpdateCertificateImageCommand(Guid.NewGuid(), invalidImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CertificateImage)
            .WithErrorMessage("Certificate image extension is not allowed.");
    }

    [Theory]
    [InlineData("test.jpg")]
    [InlineData("test.jpeg")]
    [InlineData("test.png")]
    [InlineData("test.webp")]
    [InlineData("test.JPG")]
    [InlineData("test.PNG")]
    public void UpdateCertificateImageValidator_WithAllowedExtensionsCaseInsensitive_SuccessfullyValidates(string fileName)
    {
        // Arrange
        var validImage = CreateFakeImageFile(fileName: fileName);
        var command = new UpdateCertificateImageCommand(Guid.NewGuid(), validImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("IMAGE/JPEG")]
    [InlineData("image/png")]
    [InlineData("IMAGE/WEBP")]
    public void UpdateCertificateImageValidator_WithAllowedContentTypesCaseInsensitive_SuccessfullyValidates(string contentType)
    {
        // Arrange
        var validImage = CreateFakeImageFile(contentType: contentType);
        var command = new UpdateCertificateImageCommand(Guid.NewGuid(), validImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static IFormFile CreateFakeImageFile(
        string fileName = "certificate.jpg",
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
