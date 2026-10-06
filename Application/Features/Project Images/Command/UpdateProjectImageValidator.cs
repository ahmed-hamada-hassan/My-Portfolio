using Application.Features.Project_Images.DTOs;
using Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Project_Images.Command;

internal sealed class UpdateProjectImageValidator
    : AbstractValidator<UpdateProjectImageCommand>
{
    private static readonly string[] AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    private const long MaxFileSize = 5 * 1024 * 1024;

    public UpdateProjectImageValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("ProjectId is required.");

        RuleFor(x => x.ImageId)
            .NotEmpty()
            .WithMessage("ImageId is required.");

        RuleFor(x => x.UpdateProjectImageRequest)
            .Must(HasAtLeastOneValue)
            .WithMessage("At least one field must be provided.");

        RuleFor(x => x.UpdateProjectImageRequest.Image)
            .Must(file => file is null || file.Length > 0)
            .WithMessage("Image cannot be empty.");

        RuleFor(x => x.UpdateProjectImageRequest.Image)
            .Must(file => file is null || HasAllowedExtension(file))
            .WithMessage("Only JPG, JPEG, PNG and WEBP images are allowed.");

        RuleFor(x => x.UpdateProjectImageRequest.Image)
            .Must(file => file is null || HasValidSize(file))
            .WithMessage("Image size must not exceed 5 MB.");

        RuleFor(x => x.UpdateProjectImageRequest.AltText)
            .MaximumLength(MaxLength.ProjectImageAltText)
            .WithMessage(
                $"Alt text must not exceed {MaxLength.ProjectImageAltText} characters.");

        RuleFor(x => x.UpdateProjectImageRequest.DisplayOrder)
            .InclusiveBetween((byte)1, (byte)8)
            .WithMessage("Display order must be between 1 and 8.");
    }

    private static bool HasAtLeastOneValue(UpdateProjectImageDTO request)
    {
        return request.Image is not null
            || request.AltText is not null
            || request.DisplayOrder.HasValue
            || request.IsCover.HasValue;
    }

    private static bool HasAllowedExtension(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);

        return AllowedExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase);
    }

    private static bool HasValidSize(IFormFile file)
    {
        return file.Length > 0 &&
               file.Length <= MaxFileSize;
    }
}