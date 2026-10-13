using Domain.Entities;
using Domain.Errors;
using Xunit;

namespace Domain.Tests;

public class ProjectTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessfulProjectResult()
    {
        // Arrange
        const string title = "Test Project";
        const string description = "This is a test project.";
        var startDate = new DateOnly(2023, 1, 1);
        var endDate = new DateOnly(2023, 12, 31);
        const string liveURL = "https://example.com";
        const string gitHubURL = "https://github.com/example/project";

        // Act
        var result = Project.Create(
            title, description, startDate, endDate, liveURL, gitHubURL);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var project = result.Value!;

        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.Equal(title, project.Title);
        Assert.Equal(description, project.Description);
        Assert.Equal(startDate, project.StartDate);
        Assert.Equal(endDate, project.EndDate);
        Assert.Equal(liveURL, project.LiveURL);
        Assert.Equal(gitHubURL, project.GitHubURL);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidTitle_ReturnsInvalidTitleError(
        string? title)
    {
        // Arrange
        const string description = "This is a test project.";
        var startDate = new DateOnly(2023, 1, 1);
        var endDate = new DateOnly(2023, 12, 31);

        // Act
        var result = Project.Create(
            title!, description, startDate, endDate, null, null);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProjectErrors.InvalidTitle, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidDescription_ReturnsInvalidDescriptionError(
        string? description)
    {
        // Arrange
        const string title = "Test Project";
        var startDate = new DateOnly(2023, 1, 1);
        var endDate = new DateOnly(2023, 12, 31);

        // Act
        var result = Project.Create(
            title, description!, startDate, endDate, null, null);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProjectErrors.InvalidDescription, result.Error);
    }

    [Fact]
    public void Create_WithoutEndDate_ReturnsSuccessfulProjectResult()
    {
        // Arrange
        const string title = "Test Project";
        const string description = "This is a test project.";
        var startDate = new DateOnly(2023, 1, 1);

        // Act
        var result = Project.Create(
            title, description, startDate, null, null, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var project = result.Value!;

        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.Equal(title, project.Title);
        Assert.Equal(description, project.Description);
        Assert.Equal(startDate, project.StartDate);
        Assert.Null(project.EndDate);
    }

    [Fact]
    public void Create_WhenEndDateIsBeforeStartDate_ReturnsInvalidDatesError()
    {
        // Arrange
        var startDate = new DateOnly(2023, 1, 1);
        var endDate = new DateOnly(2022, 12, 31);

        // Act
        var result = Project.Create(
            "Test Project",
            "This is a test project.",
            startDate,
            endDate,
            null,
            null);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProjectErrors.InvalidDates, result.Error);
    }

    [Fact]
    public void Update_WithValidData_UpdatesProjectSuccessfully()
    {
        // Arrange
        var project = CreateValidProject();

        const string title = "Updated Title";
        const string description = "Updated Description";
        var startDate = new DateOnly(2023, 2, 1);
        var endDate = new DateOnly(2023, 11, 30);
        const string liveURL = "https://updated-example.com";
        const string gitHubURL = "https://github.com/example/updated-project";

        // Act
        var result = project.Update(
            title, description, startDate, endDate, liveURL, gitHubURL);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(title, project.Title);
        Assert.Equal(description, project.Description);
        Assert.Equal(startDate, project.StartDate);
        Assert.Equal(endDate, project.EndDate);
        Assert.Equal(liveURL, project.LiveURL);
        Assert.Equal(gitHubURL, project.GitHubURL);
    }

    [Fact]
    public void Update_WithOnlyTitle_UpdatesTitleAndPreservesOtherProperties()
    {
        // Arrange
        var project = CreateValidProject();

        var originalDescription = project.Description;
        var originalStartDate = project.StartDate;
        var originalEndDate = project.EndDate;
        var originalLiveURL = project.LiveURL;
        var originalGitHubURL = project.GitHubURL;
        var originalSlug = project.Slug;

        const string updatedTitle = "Updated Title";

        // Act
        var result = project.Update(
            updatedTitle, null, null, null, null, null);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(updatedTitle, project.Title);
        Assert.Equal(originalDescription, project.Description);
        Assert.Equal(originalStartDate, project.StartDate);
        Assert.Equal(originalEndDate, project.EndDate);
        Assert.Equal(originalLiveURL, project.LiveURL);
        Assert.Equal(originalGitHubURL, project.GitHubURL);
        Assert.Equal(originalSlug, project.Slug);
    }

    [Fact]
    public void Update_WhenStartDateIsAfterEndDate_ReturnsInvalidDatesError()
    {
        // Arrange
        var project = CreateValidProject();

        var originalTitle = project.Title;
        var originalDescription = project.Description;
        var originalStartDate = project.StartDate;
        var originalEndDate = project.EndDate;
        var originalLiveURL = project.LiveURL;
        var originalGitHubURL = project.GitHubURL;

        var invalidStartDate = new DateOnly(2024, 1, 1);
        var endDate = new DateOnly(2023, 12, 31);

        // Act
        var result = project.Update(
            null, null, invalidStartDate, endDate, null, null);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProjectErrors.InvalidDates, result.Error);

        // Verify that a failed update doesn't mutate the entity.
        Assert.Equal(originalTitle, project.Title);
        Assert.Equal(originalDescription, project.Description);
        Assert.Equal(originalStartDate, project.StartDate);
        Assert.Equal(originalEndDate, project.EndDate);
        Assert.Equal(originalLiveURL, project.LiveURL);
        Assert.Equal(originalGitHubURL, project.GitHubURL);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidTitle_KeepsOriginalTitle(string? title)
    {
        // Arrange
        var project = CreateValidProject();
        var originalTitle = project.Title;

        // Act
        var result = project.Update(title, null, null, null, null, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalTitle, project.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidDescription_KeepsOriginalDescription(
        string? description)
    {
        // Arrange
        var project = CreateValidProject();
        var originalDescription = project.Description;

        // Act
        var result = project.Update(
            null, description, null, null, null, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalDescription, project.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidLiveURL_KeepsOriginalLiveURL(string? liveURL)
    {
        // Arrange
        var project = CreateValidProject();
        var originalLiveURL = project.LiveURL;

        // Act
        var result = project.Update(
            null, null, null, null, liveURL, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalLiveURL, project.LiveURL);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidGitHubURL_KeepsOriginalGitHubURL(
        string? gitHubURL)
    {
        // Arrange
        var project = CreateValidProject();
        var originalGitHubURL = project.GitHubURL;

        // Act
        var result = project.Update(
            null, null, null, null, null, gitHubURL);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalGitHubURL, project.GitHubURL);
    }

    [Fact]
    public void GenerateSlug_WithValidTitle_ReturnsExpectedSlug()
    {
        // Arrange
        var project = CreateValidProject();

        var expectedSlug =
            $"original-title-{project.Id.ToString("N")[..8]}";

        // Assert
        Assert.Equal(expectedSlug, project.Slug);
    }

    [Fact]
    public void GenerateSlug_WithSpecialCharacters_ReturnsExpectedSlug()
    {
        // Arrange
        const string title = "Test Project! @2023";
        var result = Project.Create(
            title,
            "This is a test project.",
            new DateOnly(2023, 1, 1),
            new DateOnly(2023, 12, 31),
            null,
            null);

        // Assert
        Assert.True(result.IsSuccess);

        var project = result.Value!;
        var expectedSlug =
            $"test-project-2023-{project.Id.ToString("N")[..8]}";

        Assert.Equal(expectedSlug, project.Slug);
    }

    [Fact]
    public void MarkAsOngoing_WhenProjectHasEndDate_ClearsEndDate()
    {
        // Arrange
        var project = CreateValidProject();

        Assert.NotNull(project.EndDate);

        // Act
        var result = project.MarkAsOngoing();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(project.EndDate);
    }

    [Fact]
    public void AddProjectImage_WithValidData_AddsImageToProject()
    {
        // Arrange
        var project = CreateValidProject();

        const string url = "https://example.com/image.jpg";
        const string publicId = "Test Image Public ID";
        const string altText = "Test Image AltText";
        const byte displayOrder = 1;

        // Act
        var result = project.AddProjectImage(
            url, publicId, altText, displayOrder, true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(project.Images);

        var image = result.Value!;

        Assert.Equal(project.Id, image.ProjectId);
        Assert.Equal(url, image.URL);
        Assert.Equal(publicId, image.PublicId);
        Assert.Equal(altText, image.AltText);
        Assert.Equal(displayOrder, image.DisplayOrder);
        Assert.True(image.IsCover);
    }

    [Fact]
    public void AddProjectImage_WhenCreationFails_DoesNotAddImageToProject()
    {
        // Arrange
        var project = CreateValidProject();

        // Assumption: ProjectImage.Create rejects an empty AltText.
        // Confirm this input against the ProjectImage domain rules.
        const string invalidAltText = "";

        // Act
        var result = project.AddProjectImage(
            "https://example.com/image.jpg",
            "Test Image Public ID",
            invalidAltText,
            1,
            true);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Empty(project.Images);
    }

    [Fact]
    public void RemoveProjectImage_WithExistingImage_RemovesImageFromProject()
    {
        // Arrange
        var project = CreateValidProject();

        var addResult = project.AddProjectImage(
            "https://example.com/image.jpg",
            "Test Image Public ID",
            "Test Image AltText",
            1,
            true);

        Assert.True(addResult.IsSuccess);

        var image = addResult.Value!;

        // Act
        var result = project.RemoveProjectImage(image);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(project.Images);
    }

    [Fact]
    public void RemoveProjectImage_WhenImageDoesNotExist_ReturnsImageNotFoundError()
    {
        // Arrange
        var project = CreateValidProject();
        var anotherProject = CreateValidProject();

        var addResult = anotherProject.AddProjectImage(
            "https://example.com/image.jpg",
            "Test Image Public ID",
            "Test Image AltText",
            1,
            true);

        Assert.True(addResult.IsSuccess);

        var image = addResult.Value!;

        // Act
        var result = project.RemoveProjectImage(image);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProjectImageErrors.ImageNotFound, result.Error);
        Assert.Empty(project.Images);
        Assert.Single(anotherProject.Images);
    }

    private static Project CreateValidProject()
    {
        var result = Project.Create(
            "Original Title",
            "Original Description",
            new DateOnly(2023, 1, 1),
            new DateOnly(2023, 12, 31),
            "https://example.com",
            "https://github.com/example/project");

        Assert.True(result.IsSuccess);

        return result.Value!;
    }
}