namespace Application.Features.Certificate.DTOs;

public sealed record GetCertificateDTO(
    Guid CertificateId,
    string CertificateTitle,
    string Issuer,
    string IssuerLogoURL,
    DateOnly IssueDate,
    string? CredentialURL,
    string ImageURL);