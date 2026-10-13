using Domain.Entities;
using Domain.Errors;

namespace Domain.Tests;

public class CertificateTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessfulCertificateResult()
    {
        // Arrange
        var name = "Test Certificate";
        var issuer = "Test Issuer";
        var issuerLogoURL = "https://example.com/logo.png";
        var issueDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var credentialURL = "https://example.com/credential";
        var imageURL = "https://example.com/image.png";
        var imagePublicId = "image_public_id";

        // Act
        var result = Certificate.Create(name, issuer, issuerLogoURL, issueDate, credentialURL, imageURL, imagePublicId);
        
        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotNull(result.Value);

        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(issuer, result.Value.Issuer);
        Assert.Equal(issuerLogoURL, result.Value.IssuerLogoURL);
        Assert.Equal(issueDate, result.Value.IssueDate);
        Assert.Equal(credentialURL, result.Value.CredentialURL);
        Assert.Equal(imageURL, result.Value.ImageURL);
        Assert.Equal(imagePublicId, result.Value.ImagePublicId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCertificateName_ReturnsInvalidCertificateNameError(string? certificateName)
    {
        // Arrange
        var issuer = "Test Issuer";
        var issuerLogoURL = "https://example.com/logo.png";
        var issueDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var credentialURL = "https://example.com/credential";
        var imageURL = "https://example.com/image.png";
        var imagePublicId = "image_public_id";

        // Act
        var result = Certificate.Create(certificateName!, issuer, issuerLogoURL, issueDate, credentialURL, imageURL, imagePublicId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CertificateErrors.InvalidName, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCertificateIssuer_ReturnsInvalidCertificateIssuerError(string? certificateIssuer)
    {
        // Arrange
        var name = "Test Certificate";
        var issuerLogoURL = "https://example.com/logo.png";
        var issueDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var credentialURL = "https://example.com/credential";
        var imageURL = "https://example.com/image.png";
        var imagePublicId = "image_public_id";

        // Act
        var result = Certificate.Create(name, certificateIssuer!, issuerLogoURL, issueDate, credentialURL, imageURL, imagePublicId);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CertificateErrors.InvalidIssuer, result.Error);
    }

    [Fact]
    public void Create_WithIssueDateInTheFuture_ReturnsIssueDateInFutureError()
    {
        // Arrange
        var name = "Test Certificate";
        var issuer = "Test Issuer";
        var issuerLogoURL = "https://example.com/logo.png";
        var issueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)); // Future date
        var credentialURL = "https://example.com/credential";
        var imageURL = "https://example.com/image.png";
        var imagePublicId = "image_public_id";
        
        // Act
        var result = Certificate.Create(name, issuer, issuerLogoURL, issueDate, credentialURL, imageURL, imagePublicId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CertificateErrors.IssueDateInFuture, result.Error);
    }

    [Fact]
    public void Create_WithDefaultIssueDate_ReturnsInvalidIssueDateRequiredError()
    {
        // Arrange
        var name = "Test Certificate";
        var issuer = "Test Issuer";
        var issuerLogoURL = "https://example.com/logo.png";
        var issueDate = default(DateOnly); // Default value
        var credentialURL = "https://example.com/credential";
        var imageURL = "https://example.com/image.png";
        var imagePublicId = "image_public_id";
        
        // Act
        var result = Certificate.Create(name, issuer, issuerLogoURL, issueDate, credentialURL, imageURL, imagePublicId);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CertificateErrors.IssueDateRequired, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCertificateIssuerLogoURL_ReturnsInvalidCertificateIssuerLogoURLError(string? certificateIssuerLogoURL)
    {
        // Arrange
        var name = "Test Certificate";
        var issuer = "Test Issuer";
        var issueDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var credentialURL = "https://example.com/credential";
        var imageURL = "https://example.com/image.png";
        var imagePublicId = "image_public_id";

        // Act
        var result = Certificate.Create(name, issuer, certificateIssuerLogoURL!, issueDate, credentialURL, imageURL, imagePublicId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CertificateErrors.InvalidIssuerLogoUrl, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCertificateImageURL_ReturnsInvalidCertificateImageURLError(string? certificateImageURL)
    {
        // Arrange
        var name = "Test Certificate";
        var issuer = "Test Issuer";
        var issuerLogoURL = "https://example.com/logo.png";
        var issueDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var credentialURL = "https://example.com/credential";
        var imagePublicId = "image_public_id";

        // Act
        var result = Certificate.Create(name, issuer, issuerLogoURL, issueDate, credentialURL, certificateImageURL!, imagePublicId);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CertificateErrors.InvalidImageUrl, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCertificateImagePublicId_ReturnsInvalidCertificateImagePublicIdError(string? certificateImagePublicId)
    {
        // Arrange
        var name = "Test Certificate";
        var issuer = "Test Issuer";
        var issuerLogoURL = "https://example.com/logo.png";
        var issueDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var credentialURL = "https://example.com/credential";
        var imageURL = "https://example.com/image.png";

        // Act
        var result = Certificate.Create(name, issuer, issuerLogoURL, issueDate, credentialURL, imageURL, certificateImagePublicId!);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CertificateErrors.InvalidImagePublicId, result.Error);
    }

    [Fact]
    public void Update_WithValidData_UpdatesCertificateSuccessfully()
    {
        // Arrange
        var certificate = CreateValidCertificate();

        var newName = "Updated Certificate Name";
        var newIssuer = "Updated Issuer";
        var newIssuerLogoURL = "https://example.com/new-logo.png";
        var newIssueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var newCredentialURL = "https://example.com/new-credential";

        // Act
        var result = certificate.Update(newName, newIssuer, newIssuerLogoURL, newIssueDate, newCredentialURL);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(newName, certificate.Name);
        Assert.Equal(newIssuer, certificate.Issuer);
        Assert.Equal(newIssuerLogoURL, certificate.IssuerLogoURL);
        Assert.Equal(newIssueDate, certificate.IssueDate);
        Assert.Equal(newCredentialURL, certificate.CredentialURL);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidCertificateName_KeepsOriginalName(string? newName)
    {
        // Arrange
        var certificate = CreateValidCertificate();

        var originalName = certificate.Name;

        var newIssuer = "Updated Issuer";
        var newIssuerLogoURL = "https://example.com/new-logo.png";
        var newIssueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var newCredentialURL = "https://example.com/new-credential";
        
        // Act
        var result = certificate.Update(newName, newIssuer, newIssuerLogoURL, newIssueDate, newCredentialURL);
        
        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(originalName, certificate.Name); // Ensure the name remains unchanged

        Assert.Equal(newIssuer, certificate.Issuer);
        Assert.Equal(newIssuerLogoURL, certificate.IssuerLogoURL);
        Assert.Equal(newIssueDate, certificate.IssueDate);
        Assert.Equal(newCredentialURL, certificate.CredentialURL);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidCertificateIssuer_KeepsOriginalIssuer(string? newIssuer)
    {
        // Arrange
        var certificate = CreateValidCertificate();
        var originalIssuer = certificate.Issuer;
        var newName = "Updated Certificate Name";
        var newIssuerLogoURL = "https://example.com/new-logo.png";
        var newIssueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var newCredentialURL = "https://example.com/new-credential";

        // Act
        var result = certificate.Update(newName, newIssuer, newIssuerLogoURL, newIssueDate, newCredentialURL);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(newName, certificate.Name);
        Assert.Equal(originalIssuer, certificate.Issuer); // Ensure the issuer remains unchanged
        Assert.Equal(newIssuerLogoURL, certificate.IssuerLogoURL);
        Assert.Equal(newIssueDate, certificate.IssueDate);
        Assert.Equal(newCredentialURL, certificate.CredentialURL);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidCertificateIssuerLogoURL_KeepsOriginalIssuerLogoURL(string? newIssuerLogoURL)
    {
        // Arrange
        var certificate = CreateValidCertificate();

        var originalIssuerLogoURL = certificate.IssuerLogoURL;

        var newName = "Updated Certificate Name";
        var newIssuer = "Updated Issuer";
        var newIssueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var newCredentialURL = "https://example.com/new-credential";
        
        // Act
        var result = certificate.Update(newName, newIssuer, newIssuerLogoURL, newIssueDate, newCredentialURL);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newName, certificate.Name);
        Assert.Equal(newIssuer, certificate.Issuer);
        Assert.Equal(originalIssuerLogoURL, certificate.IssuerLogoURL); // Ensure the issuer logo URL remains unchanged
        Assert.Equal(newIssueDate, certificate.IssueDate);
        Assert.Equal(newCredentialURL, certificate.CredentialURL);
    }

    [Fact]
    public void Update_WithIssueDateInTheFuture_ReturnsIssueDateInFutureError()
    {
        // Arrange
        var certificate = CreateValidCertificate();

        var newName = "Updated Certificate Name";
        var newIssuer = "Updated Issuer";
        var newIssuerLogoURL = "https://example.com/new-logo.png";
        var newIssueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)); // Future date
        var newCredentialURL = "https://example.com/new-credential";

        // Act
        var result = certificate.Update(newName, newIssuer, newIssuerLogoURL, newIssueDate, newCredentialURL);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CertificateErrors.IssueDateInFuture, result.Error);
    }

    [Fact]
    public void Update_WithDefaultIssueDate_ReturnsInvalidIssueDateRequiredError()
    {
        // Arrange
        var certificate = CreateValidCertificate();

        var newName = "Updated Certificate Name";
        var newIssuer = "Updated Issuer";
        var newIssuerLogoURL = "https://example.com/new-logo.png";
        var newIssueDate = default(DateOnly); // Default value
        var newCredentialURL = "https://example.com/new-credential";

        // Act
        var result = certificate.Update(newName, newIssuer, newIssuerLogoURL, newIssueDate, newCredentialURL);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CertificateErrors.IssueDateRequired, result.Error);
    }

    [Fact]
    public void Update_WithNullableIssueDate_KeepsOriginalIssueDate()
    {
        // Arrange
        var certificate = CreateValidCertificate();
        var originalIssueDate = certificate.IssueDate;
        var newName = "Updated Certificate Name";
        var newIssuer = "Updated Issuer";
        var newIssuerLogoURL = "https://example.com/new-logo.png";
        DateOnly? newIssueDate = null; // Nullable issue date
        var newCredentialURL = "https://example.com/new-credential";
        
        // Act
        var result = certificate.Update(newName, newIssuer, newIssuerLogoURL, newIssueDate, newCredentialURL);
        
        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(newName, certificate.Name);
        Assert.Equal(newIssuer, certificate.Issuer);
        Assert.Equal(newIssuerLogoURL, certificate.IssuerLogoURL);
        Assert.Equal(originalIssueDate, certificate.IssueDate); // Ensure the issue date remains unchanged
        Assert.Equal(newCredentialURL, certificate.CredentialURL);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidCertificateCredentialURL_KeepsOriginalCredentialURL(string? newCredentialURL)
    {
        // Arrange
        var certificate = CreateValidCertificate();
        var originalCredentialURL = certificate.CredentialURL;
        var newName = "Updated Certificate Name";
        var newIssuer = "Updated Issuer";
        var newIssuerLogoURL = "https://example.com/new-logo.png";
        var newIssueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        // Act
        var result = certificate.Update(newName, newIssuer, newIssuerLogoURL, newIssueDate, newCredentialURL);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newName, certificate.Name);
        Assert.Equal(newIssuer, certificate.Issuer);
        Assert.Equal(newIssuerLogoURL, certificate.IssuerLogoURL);
        Assert.Equal(newIssueDate, certificate.IssueDate);
        Assert.Equal(originalCredentialURL, certificate.CredentialURL); // Ensure the credential URL remains unchanged
    }

    [Fact]
    public void UpdateImage_WithValidDate_UpdatesCertificateImageSuccessfully()
    {
        // Arrange
        var certificate = CreateValidCertificate();
        var newImageURL = "https://example.com/new-image.png";
        var newImagePublicId = "new_image_public_id";

        // Act
        var result = certificate.UpdateImage(newImageURL, newImagePublicId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newImageURL, certificate.ImageURL);
        Assert.Equal(newImagePublicId, certificate.ImagePublicId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateImage_WithInvalidImageURL_KeepsOriginalImageURL(string? newImageURL)
    {
        // Arrange
        var certificate = CreateValidCertificate();
        var originalImageURL = certificate.ImageURL;
        var newImagePublicId = "new_image_public_id";
        
        // Act
        var result = certificate.UpdateImage(newImageURL, newImagePublicId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalImageURL, certificate.ImageURL); // Ensure the image URL remains unchanged
        Assert.Equal(newImagePublicId, certificate.ImagePublicId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateImage_WithInvalidImagePublicId_KeepsOriginalImagePublicId(string? newImagePublicId)
    {
        // Arrange
        var certificate = CreateValidCertificate();
        var originalImagePublicId = certificate.ImagePublicId;
        var newImageURL = "https://example.com/new-image.png";

        // Act
        var result = certificate.UpdateImage(newImageURL, newImagePublicId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newImageURL, certificate.ImageURL);
        Assert.Equal(originalImagePublicId, certificate.ImagePublicId); // Ensure the image public ID remains unchanged
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("   ", "   ")]
    public void UpdateImage_WithBothInvalidImageURLAndPublicId_KeepsOriginalValues(string? newImageURL, string? newImagePublicId)
    {
        // Arrange
        var certificate = CreateValidCertificate();
        var originalImageURL = certificate.ImageURL;
        var originalImagePublicId = certificate.ImagePublicId;
        // Act
        var result = certificate.UpdateImage(newImageURL, newImagePublicId);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalImageURL, certificate.ImageURL); // Ensure the image URL remains unchanged
        Assert.Equal(originalImagePublicId, certificate.ImagePublicId); // Ensure the image public ID remains unchanged
    }

    [Fact]
    public void ClearCredentialURL_SetsCredentialURLToNull()
    {
        // Arrange
        var certificate = CreateValidCertificate();

        // Act
        var result = certificate.ClearCredentialURL();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(certificate.CredentialURL);
    }


    private static Certificate CreateValidCertificate()
    {
        var name = "Test Certificate";
        var issuer = "Test Issuer";
        var issuerLogoURL = "https://example.com/logo.png";
        var issueDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var credentialURL = "https://example.com/credential";
        var imageURL = "https://example.com/image.png";
        var imagePublicId = "image_public_id";
        var result = Certificate.Create(name, issuer, issuerLogoURL, issueDate, credentialURL, imageURL, imagePublicId);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);

        return result.Value;
    }
}