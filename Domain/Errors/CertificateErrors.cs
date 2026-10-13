using Domain.Shared;

namespace Domain.Errors;

public static class CertificateErrors
{
    public static readonly Error InvalidName = new(
        "Certificate.InvalidName",
        "Certificate name cannot be empty.");

    public static readonly Error InvalidIssuer = new(
        "Certificate.InvalidIssuer",
        "Certificate issuer cannot be empty.");

    public static readonly Error InvalidIssuerLogoUrl = new(
        "Certificate.InvalidIssuerLogoUrl",
        "Certificate issuer logo URL cannot be empty.");

    public static readonly Error InvalidImageUrl = new(
        "Certificate.InvalidImageUrl",
        "Certificate image URL cannot be empty.");

    public static readonly Error InvalidImagePublicId = new(
        "Certificate.InvalidImagePublicId",
        "Certificate image public ID cannot be empty.");

    public static readonly Error CredentialUrlAlreadyExists = new(
        "Certificate.CredentialUrlAlreadyExists",
        "A certificate with the same credential URL already exists.");

    public static readonly Error IssueDateInFuture = new(
        "Certificate.IssueDateInFuture",
        "Certificate issue date cannot be in the future.");

    public static readonly Error IssueDateRequired = new(
        "Certificate.IssueDateRequired",
        "Certificate issue date is required.");

    public static readonly Error NotFound = new(
        "Certificate.NotFound",
        "Certificate not found.");
}