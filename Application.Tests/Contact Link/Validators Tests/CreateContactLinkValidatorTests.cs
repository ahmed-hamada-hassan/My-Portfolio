using Application.Features.Contact_Links.Command;
using Application.Features.Contact_Links.DTOs;
using Domain.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Contact_Link.Validators_Tests;

public class CreateContactLinkValidatorTests
{
    private readonly CreateContactLinkValidator _validator;

    public CreateContactLinkValidatorTests()
    {
        _validator = new CreateContactLinkValidator();
    }

    [Fact]
    public void CreateContactLinkValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var request = new CreateContactLinkDTO(
            Name: "LinkedIn",
            Contact: "https://linkedin.com/in/test",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateContactLinkCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com/profile")]
    [InlineData("mailto:user@example.com")]
    public void CreateContactLinkValidator_WithValidUrlSchemes_SuccessfullyValidates(string url)
    {
        // Arrange
        var request = new CreateContactLinkDTO(
            Name: "ValidLink",
            Contact: url,
            DisplayOrder: 2,
            IsVisible: true
        );
        var command = new CreateContactLinkCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateContactLinkValidator_WithAllNullFields_FailsValidation()
    {
        // Arrange
        var request = new CreateContactLinkDTO(
            Name: null!,
            Contact: null!,
            DisplayOrder: 0,
            IsVisible: false
        );
        var command = new CreateContactLinkCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateContactLinkRequest.Name)
            .WithErrorMessage("Name cannot be empty.");
        result.ShouldHaveValidationErrorFor(x => x.CreateContactLinkRequest.Contact)
            .WithErrorMessage("Contact cannot be empty.");
        result.ShouldHaveValidationErrorFor(x => x.CreateContactLinkRequest.DisplayOrder)
            .WithErrorMessage("Display order cannot be empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateContactLinkValidator_WithInvalidName_FailsValidation(string? name)
    {
        // Arrange
        var request = new CreateContactLinkDTO(
            Name: name!,
            Contact: "https://example.com",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateContactLinkCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateContactLinkRequest.Name)
            .WithErrorMessage("Name cannot be empty.");
    }

    [Fact]
    public void CreateContactLinkValidator_WithNameExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longName = new string('A', MaxLength.ContactLinkName + 1);
        var request = new CreateContactLinkDTO(
            Name: longName,
            Contact: "https://example.com",
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateContactLinkCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateContactLinkRequest.Name)
            .WithErrorMessage($"Name cannot exceed {MaxLength.ContactLinkName} characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateContactLinkValidator_WithEmptyContact_FailsValidation(string? contact)
    {
        // Arrange
        var request = new CreateContactLinkDTO(
            Name: "LinkedIn",
            Contact: contact!,
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateContactLinkCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateContactLinkRequest.Contact)
            .WithErrorMessage("Contact cannot be empty.");
    }

    [Fact]
    public void CreateContactLinkValidator_WithContactExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longContact = "https://example.com/" + new string('A', MaxLength.ContactLinkContact);
        var request = new CreateContactLinkDTO(
            Name: "LinkedIn",
            Contact: longContact,
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateContactLinkCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateContactLinkRequest.Contact)
            .WithErrorMessage($"Contact cannot exceed {MaxLength.ContactLinkContact} characters.");
    }

    [Theory]
    [InlineData("invalid-url")]
    [InlineData("ftp://example.com")]
    [InlineData("just text")]
    public void CreateContactLinkValidator_WithInvalidUrlSchemeOrFormat_FailsValidation(string invalidUrl)
    {
        // Arrange
        var request = new CreateContactLinkDTO(
            Name: "LinkedIn",
            Contact: invalidUrl,
            DisplayOrder: 1,
            IsVisible: true
        );
        var command = new CreateContactLinkCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateContactLinkRequest.Contact)
            .WithErrorMessage("Contact must be a valid URL.");
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)11)]
    [InlineData((byte)255)]
    public void CreateContactLinkValidator_WithInvalidDisplayOrder_FailsValidation(byte displayOrder)
    {
        // Arrange
        var request = new CreateContactLinkDTO(
            Name: "LinkedIn",
            Contact: "https://example.com",
            DisplayOrder: displayOrder,
            IsVisible: true
        );
        var command = new CreateContactLinkCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CreateContactLinkRequest.DisplayOrder);
    }
}
