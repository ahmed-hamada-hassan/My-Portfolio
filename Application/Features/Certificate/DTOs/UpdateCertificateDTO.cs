namespace Application.Features.Certificate.DTOs;

public sealed record UpdateCertificateDTO(string? Name, string? Issuer, string? IssuerLogoURL, DateOnly? IssueDate, string? CredentialURL);