using Domain.Errors;
using Domain.Shared;

namespace Domain.Entities;
public class Certificate
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Issuer { get; private set; } = null!;
    public string IssuerLogoURL { get; private set; } = null!;
    public DateOnly IssueDate { get; private set; }
    public string CredentialURL { get; private set; } = null!;
    public string ImageURL { get; private set; } = null!;
    public byte DisplayOrder { get; private set; }

    protected Certificate() { }

    private Certificate(string name, string issuer, string issuerLogoURL, DateOnly issueDate, string credentialURL, string imageURL, byte displayOrder)
    {
        Id = Guid.NewGuid();
        Name = name;
        Issuer = issuer;
        IssuerLogoURL = issuerLogoURL;
        IssueDate = issueDate;
        CredentialURL = credentialURL;
        ImageURL = imageURL;
        DisplayOrder = displayOrder;
    }

    public static Result<Certificate> Create(string name, string issuer, string issuerLogoURL, DateOnly issueDate, string credentialURL,
        string imageURL, byte displayOrder)
    {
        if(string.IsNullOrWhiteSpace(name))
            return Result<Certificate>.Failure(CertificateErrors.InvalidCertificateName);

        if (string.IsNullOrWhiteSpace(issuer))
            return Result<Certificate>.Failure(CertificateErrors.InvalidCertificateIssuer);

        if (string.IsNullOrWhiteSpace(issuerLogoURL))
            return Result<Certificate>.Failure(CertificateErrors.InvalidCertificateIssuerLogoURL);

        if (string.IsNullOrWhiteSpace(credentialURL))
            return Result<Certificate>.Failure(CertificateErrors.InvalidCertificateCredentialURL);

        if (string.IsNullOrWhiteSpace(imageURL))
            return Result<Certificate>.Failure(CertificateErrors.InvalidCertificateImageURL);

        var certificate = new Certificate(name, issuer, issuerLogoURL, issueDate, credentialURL, imageURL, displayOrder);

        return Result<Certificate>.Success(certificate);
    }
}
