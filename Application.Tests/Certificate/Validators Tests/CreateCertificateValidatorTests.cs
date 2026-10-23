using Application.Features.Certificate.Command;
using Application.Features.Certificate.DTOs;
using Application.Options;
using Domain.Constants;
using FakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Tests.Certificate.Validators_Tests;

public class CreateCertificateValidatorTests
{
    private readonly IOptionsSnapshot<SiteSettingsOptions> _optionsSnapshot;
    private readonly SiteSettingsOptions _siteSettingsOptions;
    private readonly CreateCertificateValidator _validator;

    public CreateCertificateValidatorTests()
    {
        _siteSettingsOptions = new SiteSettingsOptions
        {
            Images = new ImageUploadOptions
            {
                MaxFileSizeInBytes = 5 * 1024 * 1024, // 5 MB
                AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"],
                AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"]
            }
        };

        _optionsSnapshot = A.Fake<IOptionsSnapshot<SiteSettingsOptions>>();
        A.CallTo(() => _optionsSnapshot.Value).Returns(_siteSettingsOptions);

        _validator = new CreateCertificateValidator(_optionsSnapshot);
    }

    [Fact]
    public void CreateCertificateValidator_WithValidData_SuccessfullyValidates()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateCertificateValidator_WithNullOrWhitespaceCredentialURL_SuccessfullyValidates(string? credentialURL)
    {
        // Arrange
        var command = CreateValidCommand(credentialURL: credentialURL);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateCertificateValidator_WithAllNullFields_FailsValidation()
    {
        // Arrange
        var command = new CreateCertificateCommand(
            new CreateCertificateDTO(
                Name: null!,
                Issuer: null!,
                IssuerLogoURL: null!,
                IssueDate: default,
                CredentialURL: null,
                Image: null!
            )
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.Name)
            .WithErrorMessage("Certificate name is required.");
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.Issuer)
            .WithErrorMessage("Issuer is required.");
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.IssuerLogoURL)
            .WithErrorMessage("Issuer logo URL is required.");
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.IssueDate)
            .WithErrorMessage("Issue date is required.");
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.Image)
            .WithErrorMessage("Image is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateCertificateValidator_WithInvalidName_FailsValidation(string? name)
    {
        // Arrange
        var command = CreateValidCommand(name: name!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.Name)
            .WithErrorMessage("Certificate name is required.");
    }

    [Fact]
    public void CreateCertificateValidator_WithNameExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longName = new string('A', MaxLength.CertificateName + 1);
        var command = CreateValidCommand(name: longName);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.Name)
            .WithErrorMessage($"Certificate name must not exceed {MaxLength.CertificateName} characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateCertificateValidator_WithInvalidIssuer_FailsValidation(string? issuer)
    {
        // Arrange
        var command = CreateValidCommand(issuer: issuer!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.Issuer)
            .WithErrorMessage("Issuer is required.");
    }

    [Fact]
    public void CreateCertificateValidator_WithIssuerExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longIssuer = new string('A', MaxLength.CertificateIssuer + 1);
        var command = CreateValidCommand(issuer: longIssuer);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.Issuer)
            .WithErrorMessage($"Issuer must not exceed {MaxLength.CertificateIssuer} characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateCertificateValidator_WithInvalidIssuerLogoURL_FailsValidation(string? issuerLogoURL)
    {
        // Arrange
        var command = CreateValidCommand(issuerLogoURL: issuerLogoURL!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.IssuerLogoURL)
            .WithErrorMessage("Issuer logo URL is required.");
    }

    [Fact]
    public void CreateCertificateValidator_WithIssuerLogoURLExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longIssuerLogoURL = new string('A', MaxLength.CertificateIssuerLogoURL + 1);
        var command = CreateValidCommand(issuerLogoURL: longIssuerLogoURL);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.IssuerLogoURL)
            .WithErrorMessage($"Issuer logo URL must not exceed {MaxLength.CertificateIssuerLogoURL} characters.");
    }

    [Fact]
    public void CreateCertificateValidator_WithCredentialURLExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var longCredentialURL = new string('A', MaxLength.CertificateCredentialURL + 1);
        var command = CreateValidCommand(credentialURL: longCredentialURL);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.CredentialURL)
            .WithErrorMessage($"Credential URL must not exceed {MaxLength.CertificateCredentialURL} characters.");
    }

    [Fact]
    public void CreateCertificateValidator_WithDefaultIssueDate_FailsValidation()
    {
        // Arrange
        var command = CreateValidCommand(issueDate: DateOnly.MinValue);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.IssueDate)
            .WithErrorMessage("Issue date is required.");
    }

    [Fact]
    public void CreateCertificateValidator_WithFutureIssueDate_FailsValidation()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var command = CreateValidCommand(issueDate: futureDate);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.IssueDate)
            .WithErrorMessage("Issue date cannot be in the future.");
    }

    [Fact]
    public void CreateCertificateValidator_WithNullImage_FailsValidation()
    {
        // Arrange
        var request = new CreateCertificateDTO(
            Name: "Valid Certificate Name",
            Issuer: "Valid Issuer",
            IssuerLogoURL: "https://example.com/logo.png",
            IssueDate: DateOnly.FromDateTime(DateTime.UtcNow),
            CredentialURL: "https://example.com/credential",
            Image: null!
        );
        var command = new CreateCertificateCommand(request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.Image)
            .WithErrorMessage("Image is required.");
    }

    [Fact]
    public void CreateCertificateValidator_WithEmptyImage_FailsValidation()
    {
        // Arrange
        var emptyImage = CreateFakeImageFile(length: 0);
        var command = CreateValidCommand(image: emptyImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.Image)
            .WithErrorMessage("Image cannot be empty.");
    }

    [Fact]
    public void CreateCertificateValidator_WithImageExceedingMaxFileSize_FailsValidation()
    {
        // Arrange
        var oversizedImage = CreateFakeImageFile(length: _siteSettingsOptions.Images.MaxFileSizeInBytes + 1);
        var command = CreateValidCommand(image: oversizedImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.Image)
            .WithErrorMessage($"Image size must not exceed {_siteSettingsOptions.Images.MaxFileSizeInBytes / 1024 / 1024} MB.");
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    [InlineData("image/gif")]
    public void CreateCertificateValidator_WithDisallowedContentType_FailsValidation(string contentType)
    {
        // Arrange
        var invalidContentTypeImage = CreateFakeImageFile(contentType: contentType);
        var command = CreateValidCommand(image: invalidContentTypeImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.Image)
            .WithErrorMessage("Image content type is not allowed.");
    }

    [Theory]
    [InlineData("image.gif")]
    [InlineData("document.pdf")]
    [InlineData("image.bmp")]
    [InlineData("image")]
    public void CreateCertificateValidator_WithDisallowedExtension_FailsValidation(string fileName)
    {
        // Arrange
        var invalidExtensionImage = CreateFakeImageFile(fileName: fileName);
        var command = CreateValidCommand(image: invalidExtensionImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.createCertificateRequest.Image)
            .WithErrorMessage("Only JPG, JPEG, PNG and WEBP images are allowed.");
    }

    [Theory]
    [InlineData("test.jpg")]
    [InlineData("test.jpeg")]
    [InlineData("test.png")]
    [InlineData("test.webp")]
    [InlineData("test.JPG")]
    [InlineData("test.JPEG")]
    [InlineData("test.PNG")]
    [InlineData("test.WEBP")]
    public void CreateCertificateValidator_WithAllowedExtensionsCaseInsensitive_SuccessfullyValidates(string fileName)
    {
        // Arrange
        var validImage = CreateFakeImageFile(fileName: fileName);
        var command = CreateValidCommand(image: validImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("IMAGE/JPEG")]
    [InlineData("image/png")]
    [InlineData("IMAGE/PNG")]
    [InlineData("image/webp")]
    [InlineData("IMAGE/WEBP")]
    public void CreateCertificateValidator_WithAllowedContentTypesCaseInsensitive_SuccessfullyValidates(string contentType)
    {
        // Arrange
        var validImage = CreateFakeImageFile(contentType: contentType);
        var command = CreateValidCommand(image: validImage);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static IFormFile CreateFakeImageFile(
        string fileName = "test-image.jpg",
        long length = 1024,
        string contentType = "image/jpeg")
    {
        var file = A.Fake<IFormFile>();
        A.CallTo(() => file.FileName).Returns(fileName);
        A.CallTo(() => file.Length).Returns(length);
        A.CallTo(() => file.ContentType).Returns(contentType);
        return file;
    }

    private static CreateCertificateCommand CreateValidCommand(
        string name = "Valid Certificate Name",
        string issuer = "Valid Issuer",
        string issuerLogoURL = "https://example.com/logo.png",
        DateOnly? issueDate = null,
        string? credentialURL = "https://example.com/credential",
        IFormFile? image = null)
    {
        var request = new CreateCertificateDTO(
            Name: name,
            Issuer: issuer,
            IssuerLogoURL: issuerLogoURL,
            IssueDate: issueDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            CredentialURL: credentialURL,
            Image: image ?? CreateFakeImageFile()
        );

        return new CreateCertificateCommand(request);
    }
}
