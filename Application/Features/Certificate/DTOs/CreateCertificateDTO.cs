using Microsoft.AspNetCore.Http;

namespace Application.Features.Certificate.DTOs;

public sealed record CreateCertificateDTO(
    string Name,
    string Issuer,
    string IssuerLogoURL,
    DateOnly IssueDate,
    string? CredentialURL,
    IFormFile Image);