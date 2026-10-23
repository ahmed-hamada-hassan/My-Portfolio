using Application.Features.Project_Images.Command;
using Application.Features.Project_Images.DTOs;
using Application.Options;
using Domain.Constants;
using FakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Tests.Project_Images.Validators_Tests;

public class UpdateProjectImageValidatorTests
{
    private readonly IOptionsSnapshot<SiteSettingsOptions> _optionsSnapshot;
    private readonly SiteSettingsOptions _siteSettingsOptions;
    private readonly UpdateProjectImageValidator _validator;

    public UpdateProjectImageValidatorTests()
    {
        _siteSettingsOptions = new SiteSettingsOptions
        {
            Images = new ImageUploadOptions
            {
                MaxFileSizeInBytes = 2 * 1024 * 1024,
                AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"],
                AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"]
            }
        };

        _optionsSnapshot = A.Fake<IOptionsSnapshot<SiteSettingsOptions>>();
        A.CallTo(() => _optionsSnapshot.Value).Returns(_siteSettingsOptions);

        _validator = new UpdateProjectImageValidator(_optionsSnapshot);
    }

    [Fact]
    public void UpdateProjectImageValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var fakeFile = CreateFakeFile();
        var request = new UpdateProjectImageDTO(fakeFile, "Alt text", 1, true);
        var command = new UpdateProjectImageCommand(Guid.NewGuid(), Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateProjectImageValidator_WithPartialFields_SuccessfullyValidates()
    {
        // Arrange
        var request = new UpdateProjectImageDTO(null, "Updated Alt", null, null);
        var command = new UpdateProjectImageCommand(Guid.NewGuid(), Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateProjectImageValidator_WithEmptyProjectId_FailsValidation()
    {
        // Arrange
        var request = new UpdateProjectImageDTO(null, "Alt", 1, true);
        var command = new UpdateProjectImageCommand(Guid.Empty, Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.ProjectId)
            .WithErrorMessage("Project ID is required.");
    }

    [Fact]
    public void UpdateProjectImageValidator_WithEmptyImageId_FailsValidation()
    {
        // Arrange
        var request = new UpdateProjectImageDTO(null, "Alt", 1, true);
        var command = new UpdateProjectImageCommand(Guid.NewGuid(), Guid.Empty, request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.ImageId)
            .WithErrorMessage("Image ID is required.");
    }

    [Fact]
    public void UpdateProjectImageValidator_WithNullRequest_FailsValidation()
    {
        // Arrange
        var command = new UpdateProjectImageCommand(Guid.NewGuid(), Guid.NewGuid(), null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateProjectImageRequest)
            .WithErrorMessage("Update request is required.");
    }

    [Fact]
    public void UpdateProjectImageValidator_WithAllNullFields_FailsValidation()
    {
        // Arrange
        var request = new UpdateProjectImageDTO(null, null, null, null);
        var command = new UpdateProjectImageCommand(Guid.NewGuid(), Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateProjectImageRequest)
            .WithErrorMessage("At least one field must be provided.");
    }

    [Fact]
    public void UpdateProjectImageValidator_WithEmptyImage_FailsValidation()
    {
        // Arrange
        var emptyFile = CreateFakeFile(length: 0);
        var request = new UpdateProjectImageDTO(emptyFile, null, null, null);
        var command = new UpdateProjectImageCommand(Guid.NewGuid(), Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateProjectImageRequest.Image)
            .WithErrorMessage("Image cannot be empty.");
    }

    [Fact]
    public void UpdateProjectImageValidator_WithImageExceedingMaxFileSize_FailsValidation()
    {
        // Arrange
        var oversizedFile = CreateFakeFile(length: _siteSettingsOptions.Images.MaxFileSizeInBytes + 1);
        var request = new UpdateProjectImageDTO(oversizedFile, null, null, null);
        var command = new UpdateProjectImageCommand(Guid.NewGuid(), Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateProjectImageRequest.Image)
            .WithErrorMessage($"Image size must not exceed {_siteSettingsOptions.Images.MaxFileSizeInBytes / 1024 / 1024} MB.");
    }

    [Theory]
    [InlineData("image.gif")]
    [InlineData("document.pdf")]
    public void UpdateProjectImageValidator_WithDisallowedExtension_FailsValidation(string fileName)
    {
        // Arrange
        var invalidFile = CreateFakeFile(fileName: fileName);
        var request = new UpdateProjectImageDTO(invalidFile, null, null, null);
        var command = new UpdateProjectImageCommand(Guid.NewGuid(), Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateProjectImageRequest.Image)
            .WithErrorMessage("Image extension is not allowed.");
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    public void UpdateProjectImageValidator_WithDisallowedContentType_FailsValidation(string contentType)
    {
        // Arrange
        var invalidFile = CreateFakeFile(contentType: contentType);
        var request = new UpdateProjectImageDTO(invalidFile, null, null, null);
        var command = new UpdateProjectImageCommand(Guid.NewGuid(), Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateProjectImageRequest.Image)
            .WithErrorMessage("Image content type is not allowed.");
    }

    [Fact]
    public void UpdateProjectImageValidator_WithAltTextExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longAlt = new string('A', MaxLength.ProjectImageAltText + 1);
        var request = new UpdateProjectImageDTO(null, longAlt, null, null);
        var command = new UpdateProjectImageCommand(Guid.NewGuid(), Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateProjectImageRequest.AltText)
            .WithErrorMessage($"Alt text must not exceed {MaxLength.ProjectImageAltText} characters.");
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)9)]
    public void UpdateProjectImageValidator_WithInvalidDisplayOrder_FailsValidation(byte displayOrder)
    {
        // Arrange
        var request = new UpdateProjectImageDTO(null, null, displayOrder, null);
        var command = new UpdateProjectImageCommand(Guid.NewGuid(), Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateProjectImageRequest.DisplayOrder)
            .WithErrorMessage("Display order must be between 1 and 8.");
    }

    private static IFormFile CreateFakeFile(
        string fileName = "image.png",
        long length = 1024,
        string contentType = "image/png")
    {
        var file = A.Fake<IFormFile>();
        A.CallTo(() => file.FileName).Returns(fileName);
        A.CallTo(() => file.Length).Returns(length);
        A.CallTo(() => file.ContentType).Returns(contentType);
        return file;
    }
}
