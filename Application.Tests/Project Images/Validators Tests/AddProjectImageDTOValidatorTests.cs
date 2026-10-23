using Application.Features.Project_Images.DTOs;
using Application.Features.Project_Images.Validators;
using Application.Options;
using Domain.Constants;
using FakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Tests.Project_Images.Validators_Tests;

public class AddProjectImageDTOValidatorTests
{
    private readonly IOptionsSnapshot<SiteSettingsOptions> _optionsSnapshot;
    private readonly SiteSettingsOptions _siteSettingsOptions;
    private readonly AddProjectImageDTOValidator _validator;

    public AddProjectImageDTOValidatorTests()
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

        _validator = new AddProjectImageDTOValidator(_optionsSnapshot);
    }

    [Fact]
    public void AddProjectImageDTOValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var fakeFile = CreateFakeFile();
        var dto = new AddProjectImageDTO(fakeFile, "Alt text", 1, true);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AddProjectImageDTOValidator_WithNullImage_FailsValidation()
    {
        // Arrange
        var dto = new AddProjectImageDTO(null!, "Alt text", 1, true);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Image)
            .WithErrorMessage("Image is required.");
    }

    [Fact]
    public void AddProjectImageDTOValidator_WithEmptyImage_FailsValidation()
    {
        // Arrange
        var emptyFile = CreateFakeFile(length: 0);
        var dto = new AddProjectImageDTO(emptyFile, "Alt text", 1, true);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Image)
            .WithErrorMessage("Image cannot be empty.");
    }

    [Fact]
    public void AddProjectImageDTOValidator_WithImageExceedingMaxFileSize_FailsValidation()
    {
        // Arrange
        var oversizedFile = CreateFakeFile(length: _siteSettingsOptions.Images.MaxFileSizeInBytes + 1);
        var dto = new AddProjectImageDTO(oversizedFile, "Alt text", 1, true);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Image)
            .WithErrorMessage($"Image size must not exceed {_siteSettingsOptions.Images.MaxFileSizeInBytes / 1024 / 1024} MB.");
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    public void AddProjectImageDTOValidator_WithDisallowedContentType_FailsValidation(string contentType)
    {
        // Arrange
        var invalidFile = CreateFakeFile(contentType: contentType);
        var dto = new AddProjectImageDTO(invalidFile, "Alt text", 1, true);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Image)
            .WithErrorMessage("Image content type is not allowed.");
    }

    [Theory]
    [InlineData("image.gif")]
    [InlineData("document.pdf")]
    [InlineData("image.bmp")]
    public void AddProjectImageDTOValidator_WithDisallowedExtension_FailsValidation(string fileName)
    {
        // Arrange
        var invalidFile = CreateFakeFile(fileName: fileName);
        var dto = new AddProjectImageDTO(invalidFile, "Alt text", 1, true);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Image)
            .WithErrorMessage("Image extension is not allowed.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddProjectImageDTOValidator_WithEmptyAltText_FailsValidation(string? altText)
    {
        // Arrange
        var fakeFile = CreateFakeFile();
        var dto = new AddProjectImageDTO(fakeFile, altText!, 1, true);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.AltText)
            .WithErrorMessage("Alt text is required.");
    }

    [Fact]
    public void AddProjectImageDTOValidator_WithAltTextExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var fakeFile = CreateFakeFile();
        var longAlt = new string('A', MaxLength.ProjectImageAltText + 1);
        var dto = new AddProjectImageDTO(fakeFile, longAlt, 1, true);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.AltText)
            .WithErrorMessage($"Alt text must not exceed {MaxLength.ProjectImageAltText} characters.");
    }

    [Fact]
    public void AddProjectImageDTOValidator_WithZeroDisplayOrder_FailsValidation()
    {
        // Arrange
        var fakeFile = CreateFakeFile();
        var dto = new AddProjectImageDTO(fakeFile, "Alt", 0, true);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => (int)x.DisplayOrder)
            .WithErrorMessage("Display order must be greater than 0.");
    }

    [Fact]
    public void AddProjectImageDTOValidator_WithDisplayOrderGreaterThan8_FailsValidation()
    {
        // Arrange
        var fakeFile = CreateFakeFile();
        var dto = new AddProjectImageDTO(fakeFile, "Alt", 9, true);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => (int)x.DisplayOrder)
            .WithErrorMessage("Display order must not exceed 8.");
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
