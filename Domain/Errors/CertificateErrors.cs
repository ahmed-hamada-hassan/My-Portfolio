using Domain.Shared;

namespace Domain.Errors;

public class CertificateErrors
{
    public static readonly Error InvalidCertificateName = new Error("InvalidCertificateName", "Certificate name cannot be empty.");
    public static readonly Error InvalidCertificateIssuer = new Error("InvalidCertificateIssuer", "Certificate issuer cannot be empty.");
    public static readonly Error InvalidCertificateIssuerLogoURL = new Error("InvalidCertificateIssuerLogoURL", "Certificate issuer logo URL cannot be empty.");
    public static readonly Error InvalidCertificateCredentialURL = new Error("InvalidCertificateCredentialURL", "Certificate credential URL cannot be empty.");
    public static readonly Error InvalidCertificateImageURL = new Error("InvalidCertificateImageURL", "Certificate image URL cannot be empty.");
}
