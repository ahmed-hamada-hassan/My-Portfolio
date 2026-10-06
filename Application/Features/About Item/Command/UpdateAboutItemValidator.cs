using Domain.Constants;
using FluentValidation;

namespace Application.Features.About_Item.Command;

internal sealed class UpdateAboutItemValidator
    : AbstractValidator<UpdateAboutItemCommand>
{
    public UpdateAboutItemValidator()
    {
        RuleFor(x => x.AboutItemId)
            .NotEmpty();

        RuleFor(x => x.UpdateAboutItemRequest)
            .Must(request =>
                request.Title is not null ||
                request.Description is not null ||
                request.DisplayOrder.HasValue ||
                request.IsVisible.HasValue)
            .WithMessage("At least one field must be provided.");

        RuleFor(x => x.UpdateAboutItemRequest.Title)
            .NotEmpty()
            .MaximumLength(MaxLength.AboutItemTitle);

        RuleFor(x => x.UpdateAboutItemRequest.Description)
            .NotEmpty()
            .MaximumLength(MaxLength.AboutItemDescription);

        RuleFor(x => x.UpdateAboutItemRequest.DisplayOrder)
            .InclusiveBetween((byte)1, (byte)10);
    }
}