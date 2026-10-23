using Application.Features.Certificate.Command;
using Application.Features.Certificate.DTOs;
using Domain.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Certificate.Validators_Tests;

public class UpdateCertificateValidatorTests
{
    private readonly UpdateCertificateValidator _validator;

    public UpdateCertificateValidatorTests()
    {
        _validator = new UpdateCertificateValidator();
    }

    [Fact]
    public void UpdateCertificateValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var command = new UpdateCertificateCommand(
            Guid.NewGuid(),
            new UpdateCertificateDTO(
                Name: "Valid Name",
                Issuer: "Valid Issuer",
                IssuerLogoURL: "https://example.com/logo.png",
                IssueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                CredentialURL: "https://example.com/credential"
            )
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateCertificateValidator_WithEmptyCertificateId_FailsValidation()
    {
        // Arrange
        var command = new UpdateCertificateCommand(
            Guid.Empty,
            new UpdateCertificateDTO(
                Name: "Valid Name",
                Issuer: null,
                IssuerLogoURL: null,
                IssueDate: null,
                CredentialURL: null
            )
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CertificateId);
    }

    [Fact]
    public void UpdateCertificateValidator_WithAllNullFieldsExceptId_SuccessfullyValidates()
    {
        // Arrange
        var command = new UpdateCertificateCommand(
            Guid.NewGuid(),
            new UpdateCertificateDTO(
                Name: null,
                Issuer: null,
                IssuerLogoURL: null,
                IssueDate: null,
                CredentialURL: null
            )
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateCertificateValidator_WithNameExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longName = new string('A', MaxLength.CertificateName + 1);
        var command = new UpdateCertificateCommand(
            Guid.NewGuid(),
            new UpdateCertificateDTO(
                Name: longName,
                Issuer: null,
                IssuerLogoURL: null,
                IssueDate: null,
                CredentialURL: null
            )
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateCertificateRequest.Name)
            .WithErrorMessage($"Name must not exceed {MaxLength.CertificateName} characters.");
    }

    [Fact]
    public void UpdateCertificateValidator_WithIssuerExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longIssuer = new string('A', MaxLength.CertificateIssuer + 1);
        var command = new UpdateCertificateCommand(
            Guid.NewGuid(),
            new UpdateCertificateDTO(
                Name: null,
                Issuer: longIssuer,
                IssuerLogoURL: null,
                IssueDate: null,
                CredentialURL: null
            )
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateCertificateRequest.Issuer)
            .WithErrorMessage($"Issuer must not exceed {MaxLength.CertificateIssuer} characters.");
    }

    [Fact]
    public void UpdateCertificateValidator_WithIssuerLogoURLExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longIssuerLogoURL = new string('A', MaxLength.CertificateIssuerLogoURL + 1);
        var command = new UpdateCertificateCommand(
            Guid.NewGuid(),
            new UpdateCertificateDTO(
                Name: null,
                Issuer: null,
                IssuerLogoURL: longIssuerLogoURL,
                IssueDate: null,
                CredentialURL: null
            )
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateCertificateRequest.IssuerLogoURL)
            .WithErrorMessage($"Issuer logo URL must not exceed {MaxLength.CertificateIssuerLogoURL} characters.");
    }

    [Fact]
    public void UpdateCertificateValidator_WithCredentialURLExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longCredentialURL = new string('A', MaxLength.CertificateCredentialURL + 1);
        var command = new UpdateCertificateCommand(
            Guid.NewGuid(),
            new UpdateCertificateDTO(
                Name: null,
                Issuer: null,
                IssuerLogoURL: null,
                IssueDate: null,
                CredentialURL: longCredentialURL
            )
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateCertificateRequest.CredentialURL)
            .WithErrorMessage($"Credential URL must not exceed {MaxLength.CertificateCredentialURL} characters.");
    }

    [Fact]
    public void UpdateCertificateValidator_WithFutureIssueDate_FailsValidation()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var command = new UpdateCertificateCommand(
            Guid.NewGuid(),
            new UpdateCertificateDTO(
                Name: null,
                Issuer: null,
                IssuerLogoURL: null,
                IssueDate: futureDate,
                CredentialURL: null
            )
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UpdateCertificateRequest.IssueDate)
            .WithErrorMessage("Issue date must be less than or equal to the current date.");
    }
}
