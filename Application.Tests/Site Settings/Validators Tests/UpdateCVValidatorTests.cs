using Application.Features.Site_Settings.Command;
using Application.Options;
using FakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Tests.Site_Settings.Validators_Tests;

public class UpdateCVValidatorTests
{
    private readonly IOptions<SiteSettingsOptions> _options;
    private readonly SiteSettingsOptions _siteSettingsOptions;
    private readonly UpdateCVValidator _validator;

    public UpdateCVValidatorTests()
    {
        _siteSettingsOptions = new SiteSettingsOptions
        {
            CV = new DocumentUploadOptions
            {
                MaxFileSizeInBytes = 5 * 1024 * 1024, // 5 MB
                AllowedContentTypes = ["application/pdf"]
            }
        };

        _options = A.Fake<IOptions<SiteSettingsOptions>>();
        A.CallTo(() => _options.Value).Returns(_siteSettingsOptions);

        _validator = new UpdateCVValidator(_options);
    }

    [Fact]
    public void UpdateCVValidator_WithValidCV_SuccessfullyValidates()
    {
        // Arrange
        var fakeFile = CreateFakeFile();
        var command = new UpdateCVCommand(fakeFile);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateCVValidator_WithNullCV_FailsValidation()
    {
        // Arrange
        var command = new UpdateCVCommand(null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CV)
            .WithErrorMessage("CV file is required.");
    }

    [Fact]
    public void UpdateCVValidator_WithEmptyCV_FailsValidation()
    {
        // Arrange
        var emptyFile = CreateFakeFile(length: 0);
        var command = new UpdateCVCommand(emptyFile);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CV)
            .WithErrorMessage("CV file cannot be empty.");
    }

    [Fact]
    public void UpdateCVValidator_WithCVExceedingMaxFileSize_FailsValidation()
    {
        // Arrange
        var oversizedFile = CreateFakeFile(length: _siteSettingsOptions.CV.MaxFileSizeInBytes + 1);
        var command = new UpdateCVCommand(oversizedFile);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CV)
            .WithErrorMessage($"CV file size cannot exceed {_siteSettingsOptions.CV.MaxFileSizeInBytes / 1024 / 1024} MB.");
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("text/plain")]
    [InlineData("application/msword")]
    public void UpdateCVValidator_WithDisallowedContentType_FailsValidation(string contentType)
    {
        // Arrange
        var invalidFile = CreateFakeFile(contentType: contentType);
        var command = new UpdateCVCommand(invalidFile);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CV)
            .WithErrorMessage("CV file content type is not allowed.");
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("APPLICATION/PDF")]
    public void UpdateCVValidator_WithAllowedContentTypesCaseInsensitive_SuccessfullyValidates(string contentType)
    {
        // Arrange
        var validFile = CreateFakeFile(contentType: contentType);
        var command = new UpdateCVCommand(validFile);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static IFormFile CreateFakeFile(
        string fileName = "resume.pdf",
        long length = 1024,
        string contentType = "application/pdf")
    {
        var file = A.Fake<IFormFile>();
        A.CallTo(() => file.FileName).Returns(fileName);
        A.CallTo(() => file.Length).Returns(length);
        A.CallTo(() => file.ContentType).Returns(contentType);
        return file;
    }
}
