using Application.Features.Contact_Links.DTOs;
using Domain.Constants;
using FluentValidation;

namespace Application.Features.Contact_Links.Command;

internal sealed class UpdateContactLinkValidator
    : AbstractValidator<UpdateContactLinkCommand>
{
    public UpdateContactLinkValidator()
    {
        RuleFor(x => x.contactLinkId)
            .NotEmpty();

        RuleFor(x => x.updateContactLinkRequest)
            .Must(HasAtLeastOneValue)
            .WithMessage("At least one field must be provided.");

        RuleFor(x => x.updateContactLinkRequest.Name)
            .MaximumLength(MaxLength.ContactLinkName)
            .WithMessage(
                $"Name cannot exceed {MaxLength.ContactLinkName} characters.");

        RuleFor(x => x.updateContactLinkRequest.Contact)
            .MaximumLength(MaxLength.ContactLinkContact)
            .WithMessage(
                $"Contact cannot exceed {MaxLength.ContactLinkContact} characters.")
            .Must(BeValidUrl)
            .When(x => x.updateContactLinkRequest.Contact is not null)
            .WithMessage("Contact must be a valid URL.");

        RuleFor(x => x.updateContactLinkRequest.DisplayOrder)
            .InclusiveBetween((byte)1, (byte)10)
            .When(x => x.updateContactLinkRequest.DisplayOrder.HasValue)
            .WithMessage("Display order must be between 1 and 10.");
    }

    private static bool HasAtLeastOneValue(UpdateContactLinkDTO request)
    {
        return request.Name is not null
            || request.Contact is not null
            || request.DisplayOrder.HasValue
            || request.IsVisible.HasValue;
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(
            url,
            UriKind.Absolute,
            out var result)
            && (result.Scheme == Uri.UriSchemeHttp
                || result.Scheme == Uri.UriSchemeHttps
                || result.Scheme == Uri.UriSchemeMailto);
    }
}