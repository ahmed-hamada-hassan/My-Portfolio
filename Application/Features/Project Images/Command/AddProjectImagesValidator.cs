using Application.Features.Project_Images.DTOs;
using Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Project_Images.Command;

internal sealed class AddProjectImagesValidator
    : AbstractValidator<AddProjectImageDTO>
{
    private static readonly string[] AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    private const long MaxFileSize = 5 * 1024 * 1024;

    public AddProjectImagesValidator()
    {
        RuleFor(x => x.Image)
            .NotNull()
            .WithMessage("Image is required.")
            .Must(file => file is not null && file.Length > 0)
            .WithMessage("Image cannot be empty.")
            .Must(HaveAllowedExtension)
            .WithMessage("Only JPG, JPEG, PNG and WEBP images are allowed.")
            .Must(HaveValidSize)
            .WithMessage("Image size must not exceed 5 MB.");

        RuleFor(x => x.AltText)
            .NotEmpty()
            .WithMessage("Alt text is required.")
            .MaximumLength(MaxLength.ProjectImageAltText)
            .WithMessage(
                $"Alt text must not exceed {MaxLength.ProjectImageAltText} characters.");

        RuleFor(x => (int)x.DisplayOrder)
            .GreaterThan(0).WithMessage("Display order must be greater than 0.")
            .LessThanOrEqualTo(8).WithMessage("Display order must be less than or equal to 8.");
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
