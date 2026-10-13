using Domain.Entities;
using Domain.Errors;

namespace Domain.Tests;

public class ProjectImageTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessfulProjectImage()
    {
        // Arrange
        var project = CreateValidProject();

        const string imageUrl = "https://example.com/image.jpg";
        const string publicId = "image_public_id";
        const string altText = "Test Image";
        byte displayOrder = 1;
        bool isCover = true;

        // Act
        var result = ProjectImage.Create(project.Id, imageUrl, publicId, altText, displayOrder, isCover);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var projectImage = result.Value!;

        Assert.NotEqual(Guid.Empty, projectImage.Id);
        Assert.Equal(imageUrl, projectImage.URL);
        Assert.Equal(publicId, projectImage.PublicId);
        Assert.Equal(altText, projectImage.AltText);
        Assert.Equal(displayOrder, projectImage.DisplayOrder);
        Assert.Equal(isCover, projectImage.IsCover);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidImageURL_ReturnsInvalidURLError(string? url)
    {
        // Arrange
        var project = CreateValidProject();
        const string publicId = "image_public_id";
        const string altText = "Test Image";
        byte displayOrder = 1;
        bool isCover = true;

        // Act
        var result = ProjectImage.Create(project.Id, url!, publicId, altText, displayOrder, isCover);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProjectImageErrors.InvalidURL, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidImagePublicId_ReturnsInvalidPublicIdError(string? publicId)
    {
        // Arrange
        var project = CreateValidProject();
        const string imageUrl = "https://example.com/image.jpg";
        const string altText = "Test Image";
        byte displayOrder = 1;
        bool isCover = true;

        // Act
        var result = ProjectImage.Create(project.Id, imageUrl, publicId!, altText, displayOrder, isCover);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProjectImageErrors.InvalidPublicId, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidImageAletText_ReturnsInvalidAltTextError(string? altText)
    {
        // Arrange
        var project = CreateValidProject();
        const string imageUrl = "https://example.com/image.jpg";
        const string publicId = "image_public_id";
        byte displayOrder = 1;
        bool isCover = true;

        // Act
        var result = ProjectImage.Create(project.Id, imageUrl, publicId, altText!, displayOrder, isCover);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProjectImageErrors.InvalidAltText, result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    public void Create_WithInvalidDisplayOrder_ReturnsInvalidDisplayOrderError(byte displayOrder)
    {
        // Arrange
        var project = CreateValidProject();

        const string imageUrl = "https://example.com/image.jpg";
        const string publicId = "image_public_id";
        const string altText = "Test Image";
        bool isCover = true;

        // Act
        var result = ProjectImage.Create(project.Id, imageUrl, publicId, altText, displayOrder, isCover);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProjectImageErrors.InvalidDisplayOrder, result.Error);
    }

    [Fact]
    public void Update_WithValidData_UpdatesProjectImageSuccessfully()
    {
        // Arrange
        var project = CreateValidProject();
        var projectImage = CreateValidProjectImage(project);

        const string newImageUrl = "https://example.com/new_image.jpg";
        const string newPublicId = "new_image_public_id";
        const string newAltText = "Updated Test Image";
        const byte newDisplayOrder = 2;
        const bool newIsCover = false;

        // Act
        var result = projectImage.Update(newImageUrl, newPublicId, newAltText, newDisplayOrder, newIsCover);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(newImageUrl, projectImage.URL);
        Assert.Equal(newPublicId, projectImage.PublicId);
        Assert.Equal(newAltText, projectImage.AltText);
        Assert.Equal(newDisplayOrder, projectImage.DisplayOrder);
        Assert.Equal(newIsCover, projectImage.IsCover);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidImageURL_KeepsOriginalURL(string? newImageUrl)
    {
        // Arrange
        var project = CreateValidProject();
        var projectImage = CreateValidProjectImage(project);

        var originalUrl = projectImage.URL;

        const string newPublicId = "new_image_public_id";
        const string newAltText = "Updated Test Image";
        const byte newDisplayOrder = 2;
        const bool newIsCover = false;
        // Act
        var result = projectImage.Update(newImageUrl, newPublicId, newAltText, newDisplayOrder, newIsCover);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalUrl, projectImage.URL);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidImagePublicId_KeepsOriginalPublicId(string? newPublicId)
    {
        // Arrange
        var project = CreateValidProject();
        var projectImage = CreateValidProjectImage(project);

        var originalPublicId = projectImage.PublicId;

        const string newImageUrl = "https://example.com/new_image.jpg";
        const string newAltText = "Updated Test Image";
        const byte newDisplayOrder = 2;
        const bool newIsCover = false;
        // Act
        var result = projectImage.Update(newImageUrl, newPublicId, newAltText, newDisplayOrder, newIsCover);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalPublicId, projectImage.PublicId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidImageAltText_KeepsOriginalAltText(string? newAltText)
    {
        // Arrange
        var project = CreateValidProject();
        var projectImage = CreateValidProjectImage(project);

        var originalAltText = projectImage.AltText;

        const string newImageUrl = "https://example.com/new_image.jpg";
        const string newPublicId = "new_image_public_id";
        const byte newDisplayOrder = 2;
        const bool newIsCover = false;
        // Act
        var result = projectImage.Update(newImageUrl, newPublicId, newAltText, newDisplayOrder, newIsCover);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalAltText, projectImage.AltText);
    }

    [Fact]
    public void Update_WhenIsCoverDoesnotHaveValue_KeepsOriginalIsCover()
    {
        // Arrange
        var project = CreateValidProject();
        var projectImage = CreateValidProjectImage(project);

        var originalIsCover = projectImage.IsCover;

        const string newImageUrl = "https://example.com/new_image.jpg";
        const string newPublicId = "new_image_public_id";
        const string newAltText = "Updated Test Image";
        const byte newDisplayOrder = 2;
        // Act
        var result = projectImage.Update(newImageUrl, newPublicId, newAltText, newDisplayOrder, null);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalIsCover, projectImage.IsCover);
    }

    [Fact]
    public void Update_WhenDisplayOrderDoesnotHaveValue_KeepsOriginalDisplayOrder()
    {
        // Arrange
        var project = CreateValidProject();
        var projectImage = CreateValidProjectImage(project);
        var originalDisplayOrder = projectImage.DisplayOrder;
        const string newImageUrl = "https://example.com/new_image.jpg";
        const string newPublicId = "new_image_public_id";
        const string newAltText = "Updated Test Image";
        const bool newIsCover = false;
        // Act
        var result = projectImage.Update(newImageUrl, newPublicId, newAltText, null, newIsCover);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalDisplayOrder, projectImage.DisplayOrder);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    public void Update_WhenDisplayOrderHasInvalidValue_ReturnsInvalidDisplayOrderError(byte newDisplayOrder)
    {
        // Arrange
        var project = CreateValidProject();
        var projectImage = CreateValidProjectImage(project);

        var originalUrl = projectImage.URL;
        var originalPublicId = projectImage.PublicId;
        var originalAltText = projectImage.AltText;
        var originalDisplayOrder = projectImage.DisplayOrder;
        var originalIsCover = projectImage.IsCover;

        const string newImageUrl = "https://example.com/new_image.jpg";
        const string newPublicId = "new_image_public_id";
        const string newAltText = "Updated Test Image";
        const bool newIsCover = false;
        // Act
        var result = projectImage.Update(newImageUrl, newPublicId, newAltText, newDisplayOrder, newIsCover);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProjectImageErrors.InvalidDisplayOrder, result.Error);

        Assert.Equal(originalUrl, projectImage.URL);
        Assert.Equal(originalPublicId, projectImage.PublicId);
        Assert.Equal(originalAltText, projectImage.AltText);
        Assert.Equal(originalDisplayOrder, projectImage.DisplayOrder);
        Assert.Equal(originalIsCover, projectImage.IsCover);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddProjectImage_WithInvalidAltText_ReturnsInvalidAltTextError(string? altText)
    {
        // Arrange
        var project = CreateValidProject();

        // Act
        var result = project.AddProjectImage(
            "https://example.com/image.jpg",
            "image_public_id",
            altText!,
            1,
            true);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProjectImageErrors.InvalidAltText, result.Error);
        Assert.Empty(project.Images);
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

    private static ProjectImage CreateValidProjectImage(Project project)
    {
        var result = ProjectImage.Create(
            project.Id,
            "https://example.com/image.jpg",
            "image_public_id",
            "Test Image",
            1,
            true);
        Assert.True(result.IsSuccess);
        return result.Value!;
    }
}