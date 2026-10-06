using Domain.Constants;
using FluentValidation;

namespace Application.Features.Contact_Links.Command;

internal sealed class CreateContactLinkValidator : AbstractValidator<CreateContactLinkCommand>
{
    public CreateContactLinkValidator()
    {
        RuleFor(x => x.CreateContactLinkRequest.Name)
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(MaxLength.ContactLinkName).WithMessage($"Name cannot exceed {MaxLength.ContactLinkName} characters.");

        RuleFor(x => x.CreateContactLinkRequest.Contact)
            .NotEmpty().WithMessage("Contact cannot be empty.")
            .MaximumLength(MaxLength.ContactLinkContact).WithMessage($"Contact cannot exceed {MaxLength.ContactLinkContact} characters.")
            .Must(BeValidUrl).WithMessage("Contact must be a valid URL.");

        RuleFor(x => x.CreateContactLinkRequest.DisplayOrder)
            .NotEmpty().WithMessage("Display order cannot be empty.")
            .InclusiveBetween((byte)1, (byte)10).WithMessage("Display order must be between 1 and 10.");

        RuleFor(x => x.CreateContactLinkRequest.IsVisible)
            .NotNull().WithMessage("IsVisible cannot be null.");
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
