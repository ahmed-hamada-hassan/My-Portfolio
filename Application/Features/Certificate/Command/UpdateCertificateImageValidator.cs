using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Certificate.Command;

internal sealed class UpdateCertificateImageValidator : AbstractValidator<UpdateCertificateImageCommand>
{
    private static readonly string[] AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    private const long MaxFileSize = 5 * 1024 * 1024;

    public UpdateCertificateImageValidator()
    {
        RuleFor(x => x.CertificateImage)
            .NotNull()
            .WithMessage("Certificate image is required.")
            .Must(file => file is not null && file.Length > 0)
            .WithMessage("Certificate image cannot be empty.")
            .Must(HaveAllowedExtension)
            .WithMessage("Only JPG, JPEG, PNG and WEBP images are allowed.")
            .Must(HaveValidSize)
            .WithMessage("Certificate image size must not exceed 5 MB.");
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