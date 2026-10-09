using Application.Options;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Features.Certificate.Command;

internal sealed class UpdateCertificateImageValidator
    : AbstractValidator<UpdateCertificateImageCommand>
{
    public UpdateCertificateImageValidator(
        IOptionsSnapshot<SiteSettingsOptions> options)
    {
        var imageOptions = options.Value.Images;

        RuleFor(x => x.CertificateImage)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Certificate image is required.")
            .Must(file => file!.Length > 0)
            .WithMessage("Certificate image cannot be empty.")
            .Must(file => file!.Length <= imageOptions.MaxFileSizeInBytes)
            .WithMessage(
                $"Certificate image size must not exceed " +
                $"{imageOptions.MaxFileSizeInBytes / 1024 / 1024} MB.")
            .Must(file => imageOptions.AllowedContentTypes.Contains(
                file!.ContentType,
                StringComparer.OrdinalIgnoreCase))
            .WithMessage("Certificate image content type is not allowed.")
            .Must(file => HaveAllowedExtension(
                file,
                imageOptions.AllowedExtensions))
            .WithMessage("Certificate image extension is not allowed.");
    }

    private static bool HaveAllowedExtension(
        IFormFile? file,
        string[] allowedExtensions)
    {
        if (file is null)
            return false;

        var extension = Path.GetExtension(file.FileName);

        return allowedExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase);
    }
}