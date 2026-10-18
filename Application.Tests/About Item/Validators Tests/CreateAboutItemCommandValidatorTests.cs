using Application.Features.About_Item.Command;
using Application.Features.About_Item.DTOs;
using Domain.Constants;
using FluentValidation.TestHelper;

namespace Application.Tests.About_Item.Validators_Tests;

public class CreateAboutItemCommandValidatorTests
{
    private readonly CreateAboutItemValidator _createAboutItemValidator;

    public CreateAboutItemCommandValidatorTests()
    {
        _createAboutItemValidator = new CreateAboutItemValidator();
    }

    [Fact]
    public void CreateAboutItemValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var command = new CreateAboutItemCommand(new CreateAboutItemDTO("Valid Title", "Valid Description", 2, true));

        // Act
        var result = _createAboutItemValidator.TestValidate(command);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateAboutItemValidator_WithInvalidTitle_FailsValidation(string? title)
    {
        // Arrange
        var command = new CreateAboutItemCommand(new CreateAboutItemDTO(title!, "Valid Description", 2, true));

        // Act
        var result = _createAboutItemValidator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CreateAboutItemRequest.Title);
    }

    [Fact]
    public void CreateAboutItemValidator_WithTitleExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longTitle = new string('A', MaxLength.AboutItemTitle + 1); 
        var command = new CreateAboutItemCommand(new CreateAboutItemDTO(longTitle, "Valid Description", 2, true));
        
        // Act
        var result = _createAboutItemValidator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CreateAboutItemRequest.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateAboutItemValidator_WithInvalidDescription_FailsValidation(string? description)
    {
        // Arrange
        var command = new CreateAboutItemCommand(new CreateAboutItemDTO("Valid Title", description!, 2, true));
        
        // Act
        var result = _createAboutItemValidator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CreateAboutItemRequest.Description);
    }

    [Fact]
    public void CreateAboutItemValidator_WithDescriptionExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longDescription = new string('A', MaxLength.AboutItemDescription + 1);
        var command = new CreateAboutItemCommand(new CreateAboutItemDTO("Valid Title", longDescription, 2, true));

        // Act
        var result = _createAboutItemValidator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CreateAboutItemRequest.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void CreateAboutItemValidator_WithInvalidDisplayOrder_FailsValidation(byte displayOrder)
    {
        // Arrange
        var command = new CreateAboutItemCommand(new CreateAboutItemDTO("Valid Title", "Valid Description", displayOrder, true));
        
        // Act
        var result = _createAboutItemValidator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CreateAboutItemRequest.DisplayOrder);
    }
}