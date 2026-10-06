using Domain.Constants;
using FluentValidation;

namespace Application.Features.Freelance_Profile.Command;

internal class CreateFreelanceProfileValidator : AbstractValidator<CreateFreelanceProfileCommand>
{
    public CreateFreelanceProfileValidator()
    {
        RuleFor(x => x.createFreelanceProfileRequest.PlatformName)
            .NotEmpty()
            .MaximumLength(MaxLength.FreelanceProfilePlatformName);

        RuleFor(x => x.createFreelanceProfileRequest.ProfileURL)
            .NotEmpty()
            .MaximumLength(MaxLength.FreelanceProfileProfileURL)
            .Must(BeValidUrl)
            .WithMessage("Profile URL must be a valid HTTP or HTTPS URL.");

        RuleFor(x => x.createFreelanceProfileRequest.Icon)
            .NotEmpty()
            .MaximumLength(MaxLength.FreelanceProfileIcon)
            .Must(BeValidUrl)
            .WithMessage("Icon must be a valid HTTP or HTTPS URL.");

        RuleFor(x => x.createFreelanceProfileRequest.DisplayOrder)
            .InclusiveBetween((byte)1, (byte)10)
            .WithMessage("Display order must be between 1 and 10.");
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(
            url,
            UriKind.Absolute,
            out var result)
            && (result.Scheme == Uri.UriSchemeHttp
                || result.Scheme == Uri.UriSchemeHttps);
    }
}