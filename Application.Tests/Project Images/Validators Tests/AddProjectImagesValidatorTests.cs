using Application.Features.Project_Images.Command;
using Application.Features.Project_Images.DTOs;
using Application.Options;
using FakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Tests.Project_Images.Validators_Tests;

public class AddProjectImagesValidatorTests
{
    private readonly IOptionsSnapshot<SiteSettingsOptions> _optionsSnapshot;
    private readonly SiteSettingsOptions _siteSettingsOptions;
    private readonly AddProjectImagesValidator _validator;

    public AddProjectImagesValidatorTests()
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

        _validator = new AddProjectImagesValidator(_optionsSnapshot);
    }

    [Fact]
    public void AddProjectImagesValidator_WithValidImages_SuccessfullyValidates()
    {
        // Arrange
        var fakeFile = CreateFakeFile();
        var images = new List<AddProjectImageDTO> { new(fakeFile, "Alt text", 1, true) };
        var command = new AddProjectImagesCommand(Guid.NewGuid(), images);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AddProjectImagesValidator_WithInvalidImageInList_FailsValidation()
    {
        // Arrange
        var invalidFile = CreateFakeFile(length: 0); // Empty file
        var images = new List<AddProjectImageDTO> { new(invalidFile, "Alt text", 1, true) };
        var command = new AddProjectImagesCommand(Guid.NewGuid(), images);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor("ProjectImages[0].Image");
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
