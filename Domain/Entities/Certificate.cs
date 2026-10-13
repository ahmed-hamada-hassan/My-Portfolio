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
    public string? CredentialURL { get; private set; }
    public string ImageURL { get; private set; } = null!;
    public string ImagePublicId { get; private set; } = null!;

    protected Certificate() { }

    private Certificate(string name, string issuer, string issuerLogoURL, DateOnly issueDate, string? credentialURL, string imageURL, string imagePublicId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Issuer = issuer;
        IssuerLogoURL = issuerLogoURL;
        IssueDate = issueDate;
        CredentialURL = credentialURL;
        ImageURL = imageURL;
        ImagePublicId = imagePublicId;
    }

    public static Result<Certificate> Create(string name, string issuer, string issuerLogoURL, DateOnly issueDate, string? credentialURL,
        string imageURL, string imagePublicId)
    {
        if(string.IsNullOrWhiteSpace(name))
            return Result<Certificate>.Failure(CertificateErrors.InvalidName);

        if (string.IsNullOrWhiteSpace(issuer))
            return Result<Certificate>.Failure(CertificateErrors.InvalidIssuer);

        if (string.IsNullOrWhiteSpace(issuerLogoURL))
            return Result<Certificate>.Failure(CertificateErrors.InvalidIssuerLogoUrl);

        if(issueDate == default)
            return Result<Certificate>.Failure(CertificateErrors.IssueDateRequired);

        if(issueDate > DateOnly.FromDateTime(DateTime.UtcNow))
            return Result<Certificate>.Failure(CertificateErrors.IssueDateInFuture);


        if (string.IsNullOrWhiteSpace(imageURL))
            return Result<Certificate>.Failure(CertificateErrors.InvalidImageUrl);

        if (string.IsNullOrWhiteSpace(imagePublicId))
            return Result<Certificate>.Failure(CertificateErrors.InvalidImagePublicId);

        var certificate = new Certificate(name, issuer, issuerLogoURL, issueDate, 
            string.IsNullOrWhiteSpace(credentialURL) ? null : credentialURL, imageURL, imagePublicId);

        return Result<Certificate>.Success(certificate);
    }

    public Result Update(string? name, string? issuer, string? issuerLogoURL, DateOnly? issueDate, string? credentialURL)
    {
        if(issueDate.HasValue && issueDate.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            return Result.Failure(CertificateErrors.IssueDateInFuture);

        if(issueDate.HasValue && issueDate.Value == default)
            return Result.Failure(CertificateErrors.IssueDateRequired);

        if (!string.IsNullOrWhiteSpace(name))
            Name = name;

        if (!string.IsNullOrWhiteSpace(issuer))
            Issuer = issuer;

        if (!string.IsNullOrWhiteSpace(issuerLogoURL))
            IssuerLogoURL = issuerLogoURL;

        if (issueDate.HasValue)
            IssueDate = issueDate.Value;

        if (!string.IsNullOrWhiteSpace(credentialURL))
            CredentialURL = credentialURL;

        return Result.Success();
    }

    public Result UpdateImage(string? imageURL, string? imagePublicId)
    {
        if (!string.IsNullOrWhiteSpace(imageURL))
            ImageURL = imageURL;
        if (!string.IsNullOrWhiteSpace(imagePublicId))
            ImagePublicId = imagePublicId;

        return Result.Success();
    }

    public Result ClearCredentialURL()
    {
        CredentialURL = null;
        return Result.Success();
    }
}
