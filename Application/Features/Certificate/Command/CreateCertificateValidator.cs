using Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Certificate.Command;

internal sealed class CreateCertificateValidator : AbstractValidator<CreateCertificateCommand>
{
    private static readonly string[] AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    private const long MaxFileSize = 5 * 1024 * 1024;
    public CreateCertificateValidator()
    {
        RuleFor(x => x.createCertificateRequest.Name)
            .NotEmpty()
            .WithMessage("Certificate name is required.")
            .MaximumLength(MaxLength.CertificateName)
            .WithMessage($"Certificate name must not exceed {MaxLength.CertificateName} characters.");

        RuleFor(x => x.createCertificateRequest.Issuer)
            .NotEmpty()
            .WithMessage("Issuer is required.")
            .MaximumLength(MaxLength.CertificateIssuer)
            .WithMessage($"Issuer must not exceed {MaxLength.CertificateIssuer} characters.");

        RuleFor(x => x.createCertificateRequest.IssuerLogoURL)
            .NotEmpty()
            .WithMessage("Issuer logo URL is required.")
            .MaximumLength(MaxLength.CertificateIssuerLogoURL)
            .WithMessage($"Issuer logo URL must not exceed {MaxLength.CertificateIssuerLogoURL} characters.");

        RuleFor(x => x.createCertificateRequest.CredentialURL)
            .MaximumLength(MaxLength.CertificateCredentialURL)
            .WithMessage($"Credential URL must not exceed {MaxLength.CertificateCredentialURL} characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.createCertificateRequest.CredentialURL));

        RuleFor(x => x.createCertificateRequest.IssueDate)
            .NotEmpty()
            .WithMessage("Issue date is required.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Issue date cannot be in the future.");

        RuleFor(x => x.createCertificateRequest.Image)
            .NotNull()
            .WithMessage("Image is required.")
            .Must(file => file is not null && file.Length > 0)
            .WithMessage("Image cannot be empty.")
            .Must(HaveAllowedExtension)
            .WithMessage("Only JPG, JPEG, PNG and WEBP images are allowed.")
            .Must(HaveValidSize)
            .WithMessage("Image size must not exceed 5 MB.");
    }

    private static bool HaveAllowedExtension(IFormFile? file)
    {
        if (file is null)
            return false;

        var extension = Path.GetExtension(file.FileName);

        return AllowedExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase);
    }

    private static bool HaveValidSize(IFormFile? file)
    {
        return file is not null &&
               file.Length > 0 &&
               file.Length <= MaxFileSize;
    }
}
