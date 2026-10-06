using Domain.Constants;
using FluentValidation;

namespace Application.Features.Certificate.Command;

internal sealed class UpdateCertificateValidator : AbstractValidator<UpdateCertificateCommand>
{
    public UpdateCertificateValidator()
    {
        RuleFor(x => x.CertificateId)
            .NotEmpty().WithMessage("Certificate ID is required.")
            .NotEqual(Guid.Empty)
            .WithMessage("Certificate ID must not be empty.");

        RuleFor(x => x.UpdateCertificateRequest.Name)
            .MaximumLength(MaxLength.CertificateName)
            .WithMessage($"Name must not exceed {MaxLength.CertificateName} characters.");

        RuleFor(x => x.UpdateCertificateRequest.Issuer)
            .MaximumLength(MaxLength.CertificateIssuer)
            .WithMessage($"Issuer must not exceed {MaxLength.CertificateIssuer} characters.");

        RuleFor(x => x.UpdateCertificateRequest.IssuerLogoURL)
            .MaximumLength(MaxLength.CertificateIssuerLogoURL)
            .WithMessage($"Issuer logo URL must not exceed {MaxLength.CertificateIssuerLogoURL} characters.");

        RuleFor(x => x.UpdateCertificateRequest.CredentialURL)
            .MaximumLength(MaxLength.CertificateCredentialURL)
            .WithMessage($"Credential URL must not exceed {MaxLength.CertificateCredentialURL} characters.");

        RuleFor(x => x.UpdateCertificateRequest.IssueDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Issue date must be less than or equal to the current date.");
    }
}
