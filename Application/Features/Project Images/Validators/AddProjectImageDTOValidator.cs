using Application.Features.Project_Images.DTOs;
using Application.Options;
using Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Project_Images.Validators;

internal sealed class AddProjectImageDTOValidator
    : AbstractValidator<AddProjectImageDTO>
{
    public AddProjectImageDTOValidator(ImageUploadOptions imageOptions)
    {
        RuleFor(x => x.Image)
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
            .Must(file => HaveAllowedExtension(file, imageOptions.AllowedExtensions))
            .WithMessage("Image extension is not allowed.");

        RuleFor(x => x.AltText)
            .NotEmpty()
            .WithMessage("Alt text is required.")
            .MaximumLength(MaxLength.ProjectImageAltText)
            .WithMessage(
                $"Alt text must not exceed {MaxLength.ProjectImageAltText} characters.");

        RuleFor(x => (int)x.DisplayOrder)
            .GreaterThan(0)
            .WithMessage("Display order must be greater than 0.")
            .LessThanOrEqualTo(8)
            .WithMessage("Display order must not exceed 8.");
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