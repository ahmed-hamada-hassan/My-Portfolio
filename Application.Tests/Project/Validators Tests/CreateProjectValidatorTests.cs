using Application.Features.Project.Command;
using Application.Features.Project.DTOs;
using Application.Features.Project_Images.DTOs;
using Application.Features.Project_Images.Validators;
using Application.Options;
using Domain.Constants;
using FakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Tests.Project.Validators_Tests;

public class CreateProjectValidatorTests
{
    private readonly CreateProjectValidator _validator;

    public CreateProjectValidatorTests()
    {
        var siteSettingsOptions = new SiteSettingsOptions
        {
            Images = new ImageUploadOptions
            {
                MaxFileSizeInBytes = 5 * 1024 * 1024,
                AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"],
                AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"]
            }
        };

        var optionsSnapshot = A.Fake<IOptionsSnapshot<SiteSettingsOptions>>();
        A.CallTo(() => optionsSnapshot.Value).Returns(siteSettingsOptions);

        var imageValidator = new AddProjectImageDTOValidator(optionsSnapshot);
        _validator = new CreateProjectValidator(imageValidator);
    }

    [Fact]
    public void CreateProjectValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var fakeFile = CreateFakeImage();
        var images = new List<AddProjectImageDTO> { new(fakeFile, "Alt", 1, true) };
        var skillIds = new List<Guid> { Guid.NewGuid() };
        var request = new CreateProjectDTO(
            Title: "Valid Project",
            Description: "Valid Description",
            StartDate: new DateOnly(2025, 1, 1),
            EndDate: new DateOnly(2025, 6, 1),
            LiveURL: "https://example.com",
            GitHubURL: "https://github.com/example/repo",
            ProjectImages: images,
            SkillIds: skillIds
        );
        var command = new CreateProjectCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateProjectValidator_WithEmptyFields_FailsValidation()
    {
        // Arrange
        var request = new CreateProjectDTO(
            Title: "",
            Description: "",
            StartDate: default,
            EndDate: null,
            LiveURL: null,
            GitHubURL: "",
            ProjectImages: new List<AddProjectImageDTO>(),
            SkillIds: new List<Guid>()
        );
        var command = new CreateProjectCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateProjectRequest.Title)
            .WithErrorMessage("Title is required.");
        result.ShouldHaveValidationErrorFor(x => x.CreateProjectRequest.Description)
            .WithErrorMessage("Description is required.");
        result.ShouldHaveValidationErrorFor(x => x.CreateProjectRequest.GitHubURL)
            .WithErrorMessage("GitHub URL is required.");
        result.ShouldHaveValidationErrorFor(x => x.CreateProjectRequest.ProjectImages)
            .WithErrorMessage("At least one project image is required.");
    }

    [Fact]
    public void CreateProjectValidator_WithTitleExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longTitle = new string('A', MaxLength.ProjectTitle + 1);
        var request = CreateValidRequest() with { Title = longTitle };
        var command = new CreateProjectCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateProjectRequest.Title)
            .WithErrorMessage($"Title must not exceed {MaxLength.ProjectTitle} characters.");
    }

    [Fact]
    public void CreateProjectValidator_WithDescriptionExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longDesc = new string('A', MaxLength.ProjectDescription + 1);
        var request = CreateValidRequest() with { Description = longDesc };
        var command = new CreateProjectCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateProjectRequest.Description)
            .WithErrorMessage($"Description must not exceed {MaxLength.ProjectDescription} characters.");
    }

    [Fact]
    public void CreateProjectValidator_WithStartDateGreaterThanEndDate_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest() with
        {
            StartDate = new DateOnly(2025, 12, 1),
            EndDate = new DateOnly(2025, 1, 1)
        };
        var command = new CreateProjectCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateProjectRequest.StartDate)
            .WithErrorMessage("Start date must be less than or equal to end date.");
    }

    [Fact]
    public void CreateProjectValidator_WithGitHubURLExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longUrl = new string('A', MaxLength.ProjectGitHubURL + 1);
        var request = CreateValidRequest() with { GitHubURL = longUrl };
        var command = new CreateProjectCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateProjectRequest.GitHubURL)
            .WithErrorMessage($"GitHub URL must not exceed {MaxLength.ProjectGitHubURL} characters.");
    }

    [Fact]
    public void CreateProjectValidator_WithEmptyProjectImagesList_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest() with { ProjectImages = new List<AddProjectImageDTO>() };
        var command = new CreateProjectCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateProjectRequest.ProjectImages)
            .WithErrorMessage("At least one project image is required.");
    }

    [Fact]
    public void CreateProjectValidator_WithNoCoverImage_FailsValidation()
    {
        // Arrange
        var fakeFile = CreateFakeImage();
        var images = new List<AddProjectImageDTO>
        {
            new(fakeFile, "Alt", 1, IsCover: false)
        };
        var request = CreateValidRequest() with { ProjectImages = images };
        var command = new CreateProjectCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateProjectRequest.ProjectImages)
            .WithErrorMessage("Exactly one project image must be the cover image.");
    }

    [Fact]
    public void CreateProjectValidator_WithMultipleCoverImages_FailsValidation()
    {
        // Arrange
        var fakeFile1 = CreateFakeImage("img1.png");
        var fakeFile2 = CreateFakeImage("img2.png");
        var images = new List<AddProjectImageDTO>
        {
            new(fakeFile1, "Alt 1", 1, IsCover: true),
            new(fakeFile2, "Alt 2", 2, IsCover: true)
        };
        var request = CreateValidRequest() with { ProjectImages = images };
        var command = new CreateProjectCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateProjectRequest.ProjectImages)
            .WithErrorMessage("Exactly one project image must be the cover image.");
    }

    [Fact]
    public void CreateProjectValidator_WithDuplicateDisplayOrders_FailsValidation()
    {
        // Arrange
        var fakeFile1 = CreateFakeImage("img1.png");
        var fakeFile2 = CreateFakeImage("img2.png");
        var images = new List<AddProjectImageDTO>
        {
            new(fakeFile1, "Alt 1", 1, IsCover: true),
            new(fakeFile2, "Alt 2", 1, IsCover: false) // Duplicate display order 1
        };
        var request = CreateValidRequest() with { ProjectImages = images };
        var command = new CreateProjectCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateProjectRequest.ProjectImages)
            .WithErrorMessage("Project image display orders must be unique.");
    }

    [Fact]
    public void CreateProjectValidator_WithDuplicateSkillIds_FailsValidation()
    {
        // Arrange
        var skillId = Guid.NewGuid();
        var request = CreateValidRequest() with { SkillIds = new List<Guid> { skillId, skillId } };
        var command = new CreateProjectCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateProjectRequest.SkillIds)
            .WithErrorMessage("Skill IDs must be unique.");
    }

    [Fact]
    public void CreateProjectValidator_WithEmptyGuidSkillId_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest() with { SkillIds = new List<Guid> { Guid.Empty } };
        var command = new CreateProjectCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateProjectRequest.SkillIds)
            .WithErrorMessage("Skill ID cannot be an empty GUID.");
    }

    private static CreateProjectDTO CreateValidRequest()
    {
        var fakeFile = CreateFakeImage();
        return new CreateProjectDTO(
            Title: "Valid Project",
            Description: "Valid Description",
            StartDate: new DateOnly(2025, 1, 1),
            EndDate: new DateOnly(2025, 6, 1),
            LiveURL: "https://example.com",
            GitHubURL: "https://github.com/example/repo",
            ProjectImages: new List<AddProjectImageDTO> { new(fakeFile, "Alt", 1, true) },
            SkillIds: new List<Guid> { Guid.NewGuid() }
        );
    }

    private static IFormFile CreateFakeImage(string fileName = "image.png")
    {
        var file = A.Fake<IFormFile>();
        A.CallTo(() => file.FileName).Returns(fileName);
        A.CallTo(() => file.Length).Returns(1024);
        A.CallTo(() => file.ContentType).Returns("image/png");
        return file;
    }
}
