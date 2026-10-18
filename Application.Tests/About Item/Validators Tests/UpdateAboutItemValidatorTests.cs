using Application.Features.About_Item.Command;
using Application.Features.About_Item.DTOs;
using Domain.Constants;
using FluentValidation.TestHelper;

namespace Application.Tests.About_Item.Validators_Tests;

public class UpdateAboutItemValidatorTests
{
    private readonly UpdateAboutItemValidator _updateAboutItemValidator;

    public UpdateAboutItemValidatorTests()
    {
        _updateAboutItemValidator = new UpdateAboutItemValidator();
    }

    [Fact]
    public void UpdateAboutItemValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var command = new UpdateAboutItemCommand(
            Guid.NewGuid(),
            new UpdateAboutItemDTO("Valid Title", "Valid Description", 2, true));

        // Act
        var result = _updateAboutItemValidator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateAboutItemValidator_WithEmptyAboutItemId_FailsValidation()
    {
        // Arrange
        var command = new UpdateAboutItemCommand(
            Guid.Empty,
            new UpdateAboutItemDTO("Valid Title", "Valid Description", 2, true));

        // Act
        var result = _updateAboutItemValidator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.AboutItemId);
    }

    [Fact]
    public void UpdateAboutItemValidator_WithAllNullFields_FailsValidation()
    {
        // Arrange
        var command = new UpdateAboutItemCommand(
            Guid.NewGuid(),
            new UpdateAboutItemDTO(null, null, null, null));

        // Act
        var result = _updateAboutItemValidator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UpdateAboutItemRequest)
              .WithErrorMessage("At least one field must be provided.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateAboutItemValidator_WithInvalidTitle_FailsValidation(string invalidTitle)
    {
        // Arrange
        var command = new UpdateAboutItemCommand(
            Guid.NewGuid(),
            new UpdateAboutItemDTO(invalidTitle, "Valid Description", 2, true));

        // Act
        var result = _updateAboutItemValidator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UpdateAboutItemRequest.Title);
    }

    [Fact]
    public void UpdateAboutItemValidator_WithTitleExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longTitle = new string('A', MaxLength.AboutItemTitle + 1);
        var command = new UpdateAboutItemCommand(
            Guid.NewGuid(),
            new UpdateAboutItemDTO(longTitle, "Valid Description", 2, true));

        // Act
        var result = _updateAboutItemValidator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UpdateAboutItemRequest.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateAboutItemValidator_WithInvalidDescription_FailsValidation(string invalidDescription)
    {
        // Arrange
        var command = new UpdateAboutItemCommand(
            Guid.NewGuid(),
            new UpdateAboutItemDTO("Valid Title", invalidDescription, 2, true));

        // Act
        var result = _updateAboutItemValidator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UpdateAboutItemRequest.Description);
    }

    [Fact]
    public void UpdateAboutItemValidator_WithDescriptionExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longDescription = new string('A', MaxLength.AboutItemDescription + 1);
        var command = new UpdateAboutItemCommand(
            Guid.NewGuid(),
            new UpdateAboutItemDTO("Valid Title", longDescription, 2, true));

        // Act
        var result = _updateAboutItemValidator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UpdateAboutItemRequest.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void UpdateAboutItemValidator_WithInvalidPosition_FailsValidation(byte displayOrder)
    {
        // Arrange
        var command = new UpdateAboutItemCommand(
            Guid.NewGuid(),
            new UpdateAboutItemDTO("Valid Title", "Valid Description", displayOrder, true));

        // Act
        var result = _updateAboutItemValidator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UpdateAboutItemRequest.DisplayOrder);
    }
}