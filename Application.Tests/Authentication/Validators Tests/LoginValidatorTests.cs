using Application.Features.Authentication.Command;
using Application.Features.Authentication.DTOs;
using Domain.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Authentication.Validators_Tests;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator;

    public LoginValidatorTests()
    {
        _validator = new LoginValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new LoginCommand(
            new LoginDTO
            (
                Email : "myemail@gmail.com",
                Password : "MyPassword123!"
            )
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmptyEmail_ShouldHaveValidationError(string? email)
    {
        // Arrange
        var command = new LoginCommand(
            new LoginDTO
            (
                Email: email!,
                Password: "MyPassword123!"
            )
        );
        
        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.loginRequest.Email)
            .WithErrorMessage("Email is required.");
    }

    [Fact]
    public void Validate_InvalidEmailFormat_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand(
            new LoginDTO
            (
                Email: "invalid-email",
                Password: "MyPassword123!"
            )
        );
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(c => c.loginRequest.Email)
            .WithErrorMessage("Invalid email format.");
    }

    [Fact]
    public void Validate_EmailExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longEmail = new string('a', MaxLength.UserEmail + 1) + "@example.com"; // Assuming max length is 255
        var command = new LoginCommand(
            new LoginDTO
            (
                Email: longEmail,
                Password: "MyPassword123!"
            )
        );
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(c => c.loginRequest.Email)
            .WithErrorMessage($"Email must not exceed {MaxLength.UserEmail} characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmptyPassword_ShouldHaveValidationError(string? password)
    {
        // Arrange
        var command = new LoginCommand(
            new LoginDTO
            (
                Email: "myemail@gmail.com",
                Password: password!
            )
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.loginRequest.Password)
            .WithErrorMessage("Password is required.");
    }
}