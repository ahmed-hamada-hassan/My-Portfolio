using Application.Features.Contact_Links.Command;
using Application.Features.Contact_Links.DTOs;
using Domain.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Contact_Link.Validators_Tests;

public class UpdateContactLinkValidatorTests
{
    private readonly UpdateContactLinkValidator _validator;

    public UpdateContactLinkValidatorTests()
    {
        _validator = new UpdateContactLinkValidator();
    }

    [Fact]
    public void UpdateContactLinkValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var command = new UpdateContactLinkCommand(
            Guid.NewGuid(),
            new UpdateContactLinkDTO(
                Name: "Updated Name",
                Contact: "https://updated.com",
                DisplayOrder: 2,
                IsVisible: true
            )
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateContactLinkValidator_WithEmptyContactLinkId_FailsValidation()
    {
        // Arrange
        var command = new UpdateContactLinkCommand(
            Guid.Empty,
            new UpdateContactLinkDTO("Name", null, null, null)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.contactLinkId);
    }

    [Fact]
    public void UpdateContactLinkValidator_WithAllNullFields_FailsValidation()
    {
        // Arrange
        var command = new UpdateContactLinkCommand(
            Guid.NewGuid(),
            new UpdateContactLinkDTO(null, null, null, null)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.updateContactLinkRequest)
            .WithErrorMessage("At least one field must be provided.");
    }

    [Fact]
    public void UpdateContactLinkValidator_WithNameExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longName = new string('A', MaxLength.ContactLinkName + 1);
        var command = new UpdateContactLinkCommand(
            Guid.NewGuid(),
            new UpdateContactLinkDTO(longName, null, null, null)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.updateContactLinkRequest.Name)
            .WithErrorMessage($"Name cannot exceed {MaxLength.ContactLinkName} characters.");
    }

    [Fact]
    public void UpdateContactLinkValidator_WithContactExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longContact = "https://example.com/" + new string('A', MaxLength.ContactLinkContact);
        var command = new UpdateContactLinkCommand(
            Guid.NewGuid(),
            new UpdateContactLinkDTO(null, longContact, null, null)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.updateContactLinkRequest.Contact)
            .WithErrorMessage($"Contact cannot exceed {MaxLength.ContactLinkContact} characters.");
    }

    [Theory]
    [InlineData("invalid-url")]
    [InlineData("ftp://example.com")]
    [InlineData("plain text")]
    public void UpdateContactLinkValidator_WithInvalidContactUrl_FailsValidation(string invalidUrl)
    {
        // Arrange
        var command = new UpdateContactLinkCommand(
            Guid.NewGuid(),
            new UpdateContactLinkDTO(null, invalidUrl, null, null)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.updateContactLinkRequest.Contact)
            .WithErrorMessage("Contact must be a valid URL.");
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)11)]
    [InlineData((byte)200)]
    public void UpdateContactLinkValidator_WithInvalidDisplayOrder_FailsValidation(byte displayOrder)
    {
        // Arrange
        var command = new UpdateContactLinkCommand(
            Guid.NewGuid(),
            new UpdateContactLinkDTO(null, null, displayOrder, null)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.updateContactLinkRequest.DisplayOrder)
            .WithErrorMessage("Display order must be between 1 and 10.");
    }
}
