using Application.Options;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Application.Features.Site_Settings.Command;

internal sealed class UpdateProfileImageValidator : AbstractValidator<UpdateProfileImageCommand>
{
    public UpdateProfileImageValidator(IOptionsSnapshot<SiteSettingsOptions> siteSettingsOptions)
    {
        var imageOptions = siteSettingsOptions.Value.Images;

        RuleFor(x => x.ProfileImage)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Profile image is required.")
            .Must(file => file!.Length > 0)
            .WithMessage("Profile image cannot be empty.")
            .Must(file => file!.Length <= imageOptions.MaxFileSizeInBytes)
            .WithMessage(
                $"Profile image size cannot exceed " +
                $"{imageOptions.MaxFileSizeInBytes / 1024 / 1024} MB.")
            .Must(file => imageOptions.AllowedContentTypes
                .Contains(file!.ContentType, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Profile image content type is not allowed.");
    }
}