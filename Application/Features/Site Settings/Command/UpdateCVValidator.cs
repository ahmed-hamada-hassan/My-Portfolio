using Application.Options;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Application.Features.Site_Settings.Command;

internal sealed class UpdateCVValidator
    : AbstractValidator<UpdateCVCommand>
{
    public UpdateCVValidator(IOptions<SiteSettingsOptions> options)
    {
        var cvOptions = options.Value.CV;

        RuleFor(x => x.CV)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("CV file is required.")
            .Must(file => file!.Length > 0)
            .WithMessage("CV file cannot be empty.")
            .Must(file => file!.Length <= cvOptions.MaxFileSizeInBytes)
            .WithMessage(
                $"CV file size cannot exceed " +
                $"{cvOptions.MaxFileSizeInBytes / 1024 / 1024} MB.")
            .Must(file => cvOptions.AllowedContentTypes
                .Contains(
                    file!.ContentType,
                    StringComparer.OrdinalIgnoreCase))
            .WithMessage("CV file content type is not allowed.");
    }
}
