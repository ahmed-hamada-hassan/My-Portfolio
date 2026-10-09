using Application.Features.Project_Images.DTOs;
using Application.Options;
using Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Features.Project_Images.Command;

internal sealed class UpdateProjectImageValidator : AbstractValidator<UpdateProjectImageCommand>
{
    public UpdateProjectImageValidator(IOptionsSnapshot<SiteSettingsOptions> options)
    {
        var imageOptions = options.Value.Images;

        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.");

        RuleFor(x => x.ImageId)
            .NotEmpty()
            .WithMessage("Image ID is required.");

        RuleFor(x => x.UpdateProjectImageRequest)
            .NotNull()
            .WithMessage("Update request is required.")
            .Must(request => request is not null && HasAtLeastOneValue(request))
            .WithMessage("At least one field must be provided.");

        When(
            x => x.UpdateProjectImageRequest is not null,
            () =>
            {
                RuleFor(x => x.UpdateProjectImageRequest.Image)
                    .Cascade(CascadeMode.Stop)
                    .Must(file => file is null || file.Length > 0)
                    .WithMessage("Image cannot be empty.")
                    .Must(file => file is null ||
                                  file.Length <= imageOptions.MaxFileSizeInBytes)
                    .WithMessage(
                        $"Image size must not exceed " +
                        $"{imageOptions.MaxFileSizeInBytes / 1024 / 1024} MB.")
                    .Must(file => file is null ||
                                  HasAllowedExtension(
                                      file,
                                      imageOptions.AllowedExtensions))
                    .WithMessage("Image extension is not allowed.")
                    .Must(file => file is null ||
                                  HasAllowedContentType(
                                      file,
                                      imageOptions.AllowedContentTypes))
                    .WithMessage("Image content type is not allowed.");

                RuleFor(x => x.UpdateProjectImageRequest.AltText)
                    .MaximumLength(MaxLength.ProjectImageAltText)
                    .WithMessage(
                        $"Alt text must not exceed " +
                        $"{MaxLength.ProjectImageAltText} characters.");

                RuleFor(x => x.UpdateProjectImageRequest.DisplayOrder)
                    .InclusiveBetween((byte)1, (byte)8)
                    .When(x => x.UpdateProjectImageRequest.DisplayOrder.HasValue)
                    .WithMessage("Display order must be between 1 and 8.");
            });
    }

    private static bool HasAtLeastOneValue(UpdateProjectImageDTO request)
    {
        return request.Image is not null
            || request.AltText is not null
            || request.DisplayOrder.HasValue
            || request.IsCover.HasValue;
    }

    private static bool HasAllowedExtension(
        IFormFile file,
        string[] allowedExtensions)
    {
        var extension = Path.GetExtension(file.FileName);

        return allowedExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase);
    }

    private static bool HasAllowedContentType(
        IFormFile file,
        string[] allowedContentTypes)
    {
        return allowedContentTypes.Contains(
            file.ContentType,
            StringComparer.OrdinalIgnoreCase);
    }
}