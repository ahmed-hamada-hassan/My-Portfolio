using Domain.Entities;
using Domain.Errors;

namespace Domain.Tests;

public class ContactLinkTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessfulContactLinkResult()
    {
        // Arrange
        var name = "Name Test";
        var contact = "Contact Test";
        var displayOrder = (byte)5;
        var isVisible = true;

        // Act
        var result = ContactLink.Create(name, contact, displayOrder, isVisible);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotNull(result.Value);

        var contactLink = result.Value;

        Assert.Equal(name, contactLink.Name);
        Assert.Equal(contact, contactLink.Contact);
        Assert.Equal(displayOrder, contactLink.DisplayOrder);
        Assert.Equal(isVisible, contactLink.IsVisible);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidName_ReturnsInvalidNameError(string? invalidName)
    {
        // Arrange
        var contact = "Contact Test";
        var displayOrder = (byte)5;
        var isVisible = true;

        // Act
        var result = ContactLink.Create(invalidName!, contact, displayOrder, isVisible);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ContactLinkErrors.InvalidName, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidContact_ReturnsInvalidContactError(string? invalidContact)
    {
        // Arrange
        var name = "Name Test";
        var displayOrder = (byte)5;
        var isVisible = true;

        // Act
        var result = ContactLink.Create(name, invalidContact!, displayOrder, isVisible);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ContactLinkErrors.InvalidContact, result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Create_WithInvalidDisplayOrder_ReturnsInvalidDisplayOrderError(byte invalidDisplayOrder)
    {
        // Arrange
        var name = "Name Test";
        var contact = "Contact Test";
        var isVisible = true;

        // Act
        var result = ContactLink.Create(name, contact, invalidDisplayOrder, isVisible);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(ContactLinkErrors.InvalidDisplayOrder, result.Error);
    }

    [Fact]
    public void Update_WithValidData_UpdatesContactLinkSuccessfully()
    {
        // Arrange
        var contactLink = CreateValidContactLink();

        var newName = "Updated Name";
        var newContact = "Updated Contact";
        var newDisplayOrder = (byte)3;
        var newIsVisible = false;

        // Act
        var result = contactLink.Update(newName, newContact, newDisplayOrder, newIsVisible);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(contactLink);

        Assert.Equal(newName, contactLink.Name);
        Assert.Equal(newContact, contactLink.Contact);
        Assert.Equal(newDisplayOrder, contactLink.DisplayOrder);
        Assert.Equal(newIsVisible, contactLink.IsVisible);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WithInvalidName_KeepsOriginalName(string? newInvalidName)
    {
        // Arrange
        var contactLink = CreateValidContactLink();

        var originalName = contactLink.Name;

        var newContact = "Updated Contact";
        var newDisplayOrder = (byte)3;
        var newIsVisible = false;

        // Act
        var result = contactLink.Update(newInvalidName!, newContact, newDisplayOrder, newIsVisible);
        
        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotNull(contactLink);

        Assert.Equal(originalName, contactLink.Name);
        Assert.Equal(newContact, contactLink.Contact);
        Assert.Equal(newDisplayOrder, contactLink.DisplayOrder);
        Assert.Equal(newIsVisible, contactLink.IsVisible);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WithInvalidContact_KeepsOriginalContact(string? newInvalidContact)
    {
        // Arrange
        var contactLink = CreateValidContactLink();

        var originalContact = contactLink.Contact;

        var newName = "Updated Name";
        var newDisplayOrder = (byte)3;
        var newIsVisible = false;
        
        // Act
        var result = contactLink.Update(newName, newInvalidContact!, newDisplayOrder, newIsVisible);
        
        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotNull(contactLink);

        Assert.Equal(newName, contactLink.Name);
        Assert.Equal(originalContact, contactLink.Contact);
        Assert.Equal(newDisplayOrder, contactLink.DisplayOrder);
        Assert.Equal(newIsVisible, contactLink.IsVisible);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Update_WithInvalidDisplayOrder_KeepsOriginalDisplayOrder(byte invalidDisplayOrder)
    {
        // Arrange
        var contactLink = CreateValidContactLink();

        var originalDisplayOrder = contactLink.DisplayOrder;

        var newName = "Updated Name";
        var newContact = "Updated Contact";
        var newIsVisible = false;

        // Act
        var result = contactLink.Update(newName, newContact, invalidDisplayOrder, newIsVisible);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotNull(contactLink);

        Assert.Equal(newName, contactLink.Name);
        Assert.Equal(newContact, contactLink.Contact);
        Assert.Equal(originalDisplayOrder, contactLink.DisplayOrder);
        Assert.Equal(newIsVisible, contactLink.IsVisible);
    }

    [Fact]
    public void Update_WithNullableIsVisible_KeepsOriginalIsVisible()
    {
        // Arrange
        var contactLink = CreateValidContactLink();

        var originalIsVisible = contactLink.IsVisible;

        var newName = "Updated Name";
        var newContact = "Updated Contact";
        var newDisplayOrder = (byte)3;
        bool? newIsVisible = null;

        // Act
        var result = contactLink.Update(newName, newContact, newDisplayOrder, newIsVisible);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotNull(contactLink);

        Assert.Equal(newName, contactLink.Name);
        Assert.Equal(newContact, contactLink.Contact);
        Assert.Equal(newDisplayOrder, contactLink.DisplayOrder);
        Assert.Equal(originalIsVisible, contactLink.IsVisible);
    }

    private static ContactLink CreateValidContactLink()
    {
        var name = "Name Test";
        var contact = "Contact Test";
        var displayOrder = (byte)5;
        var isVisible = true;
        var result = ContactLink.Create(name, contact, displayOrder, isVisible);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        return result.Value;
    }
}