using Domain.Shared;

namespace Domain.Errors;

public class CertificateErrors
{
    public static readonly Error InvalidCertificateName = new Error("Certificate.InvalidName", "Certificate name cannot be empty.");
    public static readonly Error InvalidCertificateIssuer = new Error("Certificate.InvalidIssuer", "Certificate issuer cannot be empty.");
    public static readonly Error InvalidCertificateIssuerLogoURL = new Error("Certificate.InvalidIssuerLogoURL", "Certificate issuer logo URL cannot be empty.");
    public static readonly Error InvalidCertificateImageURL = new Error("Certificate.InvalidImageURL", "Certificate image URL cannot be empty.");
    public static readonly Error InvalidCertificateImagePublicId = new Error("Certificate.InvalidImagePublicId", "Certificate image public ID cannot be empty.");
    public static readonly Error CredentialURLAlreadyExists = new Error("Certificate.CredentialURLAlreadyExists", "A certificate with the same credential URL already exists.");
    public static readonly Error InvalidCertificateIssueDate = new Error("Certificate.InvalidIssueDate", "Certificate issue date cannot be in the future.");
    public static readonly Error CertificateNotFound = new Error("Certificate.NotFound", "Certificate not found.");
}
