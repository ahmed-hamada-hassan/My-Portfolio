using Application.Options;
using Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Features.Certificate.Command;

internal sealed class CreateCertificateValidator
    : AbstractValidator<CreateCertificateCommand>
{
    public CreateCertificateValidator(
        IOptionsSnapshot<SiteSettingsOptions> options)
    {
        var imageOptions = options.Value.Images;

        RuleFor(x => x.createCertificateRequest.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Certificate name is required.")
            .MaximumLength(MaxLength.CertificateName)
            .WithMessage(
                $"Certificate name must not exceed {MaxLength.CertificateName} characters.");

        RuleFor(x => x.createCertificateRequest.Issuer)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Issuer is required.")
            .MaximumLength(MaxLength.CertificateIssuer)
            .WithMessage(
                $"Issuer must not exceed {MaxLength.CertificateIssuer} characters.");

        RuleFor(x => x.createCertificateRequest.IssuerLogoURL)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Issuer logo URL is required.")
            .MaximumLength(MaxLength.CertificateIssuerLogoURL)
            .WithMessage(
                $"Issuer logo URL must not exceed {MaxLength.CertificateIssuerLogoURL} characters.");

        RuleFor(x => x.createCertificateRequest.CredentialURL)
            .MaximumLength(MaxLength.CertificateCredentialURL)
            .WithMessage(
                $"Credential URL must not exceed {MaxLength.CertificateCredentialURL} characters.")
            .When(x => !string.IsNullOrWhiteSpace(
                x.createCertificateRequest.CredentialURL));

        RuleFor(x => x.createCertificateRequest.IssueDate)
            .NotEmpty()
            .WithMessage("Issue date is required.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Issue date cannot be in the future.");

        RuleFor(x => x.createCertificateRequest.Image)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Image is required.")
            .Must(file => file!.Length > 0)
            .WithMessage("Image cannot be empty.")
            .Must(file => file!.Length <= imageOptions.MaxFileSizeInBytes)
            .WithMessage(
                $"Image size must not exceed {imageOptions.MaxFileSizeInBytes / 1024 / 1024} MB.")
            .Must(file => imageOptions.AllowedContentTypes.Contains(
                file!.ContentType,
                StringComparer.OrdinalIgnoreCase))
            .WithMessage("Image content type is not allowed.")
            .Must((file) => HaveAllowedExtension(file, imageOptions.AllowedExtensions))
            .WithMessage("Only JPG, JPEG, PNG and WEBP images are allowed.");
    }

    private static bool HaveAllowedExtension(IFormFile? file, string[] allowedExtensions)
    {
        if (file is null)
            return false;

        var extension = Path.GetExtension(file.FileName);

        return allowedExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase);
    }
}