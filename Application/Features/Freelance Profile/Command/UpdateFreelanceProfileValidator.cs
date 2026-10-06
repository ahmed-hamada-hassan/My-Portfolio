using Application.Features.Freelance_Profile.DTOs;
using Domain.Constants;
using FluentValidation;

namespace Application.Features.Freelance_Profile.Command;

internal sealed class UpdateFreelanceProfileValidator
    : AbstractValidator<UpdateFreelanceProfileCommand>
{
    public UpdateFreelanceProfileValidator()
    {
        RuleFor(x => x.FreelanceProfileId)
            .NotEmpty();

        RuleFor(x => x.UpdateFreelanceProfileRequest)
            .Must(HasAtLeastOneValue)
            .WithMessage("At least one field must be provided.");

        RuleFor(x => x.UpdateFreelanceProfileRequest.PlatformName)
            .MaximumLength(MaxLength.FreelanceProfilePlatformName);

        RuleFor(x => x.UpdateFreelanceProfileRequest.ProfileURL)
            .MaximumLength(MaxLength.FreelanceProfileProfileURL)
            .Must(BeValidUrl)
            .When(x => x.UpdateFreelanceProfileRequest.ProfileURL is not null)
            .WithMessage("Profile URL must be a valid HTTP or HTTPS URL.");

        RuleFor(x => x.UpdateFreelanceProfileRequest.Icon)
            .MaximumLength(MaxLength.FreelanceProfileIcon)
            .Must(BeValidUrl)
            .When(x => x.UpdateFreelanceProfileRequest.Icon is not null)
            .WithMessage("Icon must be a valid HTTP or HTTPS URL.");

        RuleFor(x => x.UpdateFreelanceProfileRequest.DisplayOrder)
            .InclusiveBetween((byte)1, (byte)10)
            .When(x => x.UpdateFreelanceProfileRequest.DisplayOrder.HasValue)
            .WithMessage("Display order must be between 1 and 10.");
    }

    private static bool HasAtLeastOneValue(
        UpdateFreelanceProfileDTO request)
    {
        return request.PlatformName is not null
            || request.ProfileURL is not null
            || request.Icon is not null
            || request.DisplayOrder.HasValue
            || request.IsVisible.HasValue;
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(
            url,
            UriKind.Absolute,
            out var result)
            && (result.Scheme == Uri.UriSchemeHttp
                || result.Scheme == Uri.UriSchemeHttps);
    }
}