using Domain.Entities;
using Domain.Errors;

namespace Domain.Tests;

public class AboutItemTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessfullyAboutItemResult()
    {
        // Arrange
        var title = "Test Title";
        var description = "Test Description";
        byte displayOrder = 1;
        bool isVisible = true;

        // Act
        var result = AboutItem.Create(title, description, displayOrder, isVisible);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotNull(result.Value);

        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(title, result.Value.Title);
        Assert.Equal(description, result.Value.Description);
        Assert.Equal(displayOrder, result.Value.DisplayOrder);
        Assert.Equal(isVisible, result.Value.IsVisible);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidTitle_ReturnsInvalidTitleError(string? invalidTitle)
    {
        // Arrange
        var description = "Test Description";
        byte displayOrder = 1;
        bool isVisible = true;

        // Act
        var result = AboutItem.Create(invalidTitle!, description, displayOrder, isVisible);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(AboutItemErrors.InvalidTitle, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidDescription_ReturnsInvalidDescriptionError(string? invalidDescription)
    {
        // Arrange
        var title = "Test Title";
        byte displayOrder = 1;
        bool isVisible = true;

        // Act
        var result = AboutItem.Create(title, invalidDescription!, displayOrder, isVisible);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(AboutItemErrors.InvalidDescription, result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Create_WithInvalidDisplayOrder_ReturnsInvalidDisplayOrderError(byte invalidDisplayOrder)
    {
        // Arrange
        var title = "Test Title";
        var description = "Test Description";
        bool isVisible = true;

        // Act
        var result = AboutItem.Create(title, description, invalidDisplayOrder, isVisible);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(AboutItemErrors.InvalidDisplayOrder, result.Error);
    }

    [Fact]
    public void Update_WithValidData_UpdatesSuccessfullyAboutItem()
    {
        // Arrange
        var aboutItem = CreateValidAboutItem();
        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        byte newDisplayOrder = 2;
        bool newIsVisible = false;

        // Act
        var result = aboutItem.Update(newTitle, newDescription, newDisplayOrder, newIsVisible);
        
        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(newTitle, aboutItem.Title);
        Assert.Equal(newDescription, aboutItem.Description);
        Assert.Equal(newDisplayOrder, aboutItem.DisplayOrder);
        Assert.Equal(newIsVisible, aboutItem.IsVisible);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WithInvalidTitle_KeepsOriginalTitle(string? newInvalidTitle)
    {
        // Arrange
        var aboutItem = CreateValidAboutItem();

        var originalTitle = aboutItem.Title;

        var newDescription = "Updated Description";
        byte newDisplayOrder = 2;
        bool newIsVisible = false;

        // Act
        var result = aboutItem.Update(newInvalidTitle, newDescription, newDisplayOrder, newIsVisible);
        
        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(originalTitle, aboutItem.Title);
        Assert.Equal(newDescription, aboutItem.Description);
        Assert.Equal(newDisplayOrder, aboutItem.DisplayOrder);
        Assert.Equal(newIsVisible, aboutItem.IsVisible);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WithInvalidDescription_KeepsOriginalDescription(string? newInvalidDescription)
    {
        // Arrange
        var aboutItem = CreateValidAboutItem();

        var originalDescription = aboutItem.Description;

        var newTitle = "Updated Title";
        byte newDisplayOrder = 2;
        bool newIsVisible = false;

        // Act
        var result = aboutItem.Update(newTitle, newInvalidDescription, newDisplayOrder, newIsVisible);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(newTitle, aboutItem.Title);
        Assert.Equal(originalDescription, aboutItem.Description);
        Assert.Equal(newDisplayOrder, aboutItem.DisplayOrder);
        Assert.Equal(newIsVisible, aboutItem.IsVisible);
    }

    [Fact]
    public void Update_WithInvalidDisplayOrder_KeepsOriginalDisplayOrder()
    {
        // Arrange
        var aboutItem = CreateValidAboutItem();

        var originalDisplayOrder = aboutItem.DisplayOrder;

        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        byte newInvalidDisplayOrder = 0;
        bool newIsVisible = false;

        // Act
        var result = aboutItem.Update(newTitle, newDescription, newInvalidDisplayOrder, newIsVisible);
        
        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(newTitle, aboutItem.Title);
        Assert.Equal(newDescription, aboutItem.Description);
        Assert.Equal(originalDisplayOrder, aboutItem.DisplayOrder);
        Assert.Equal(newIsVisible, aboutItem.IsVisible);
    }

    [Fact]
    public void Update_WithInvalidIsVisible_KeepsOriginalIsVisible()
    {
        // Arrange
        var aboutItem = CreateValidAboutItem();

        var originalIsVisible = aboutItem.IsVisible;

        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        byte newDisplayOrder = 2;
        bool? newInvalidIsVisible = null;
        // Act
        var result = aboutItem.Update(newTitle, newDescription, newDisplayOrder, newInvalidIsVisible);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(newTitle, aboutItem.Title);
        Assert.Equal(newDescription, aboutItem.Description);
        Assert.Equal(newDisplayOrder, aboutItem.DisplayOrder);
        Assert.Equal(originalIsVisible, aboutItem.IsVisible);
    }

    private static AboutItem CreateValidAboutItem()
    {
        var title = "Test Title";
        var description = "Test Description";
        byte displayOrder = 1;
        bool isVisible = true;
        var result = AboutItem.Create(title, description, displayOrder, isVisible);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.NotEqual(Guid.Empty, result.Value.Id);

        return result.Value;
    }
}