using Domain.Constants;
using FluentValidation;

namespace Application.Features.About_Item.Command;

internal sealed class CreateAboutItemValidator
    : AbstractValidator<CreateAboutItemCommand>
{
    public CreateAboutItemValidator()
    {
        RuleFor(x => x.CreateAboutItemRequest.Title)
            .NotEmpty()
            .MaximumLength(MaxLength.AboutItemTitle);

        RuleFor(x => x.CreateAboutItemRequest.Description)
            .NotEmpty()
            .MaximumLength(MaxLength.AboutItemDescription);

        RuleFor(x => x.CreateAboutItemRequest.DisplayOrder)
            .InclusiveBetween((byte)1, (byte)10);
    }
}