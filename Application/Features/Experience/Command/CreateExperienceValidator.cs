using Domain.Constants;
using FluentValidation;

namespace Application.Features.Experience.Command;

internal sealed class CreateExperienceValidator : AbstractValidator<CreateExperienceCommand>
{
    public CreateExperienceValidator()
    {
        RuleFor(x => x.createExperienceRequest.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(MaxLength.ExperienceCompanyName).WithMessage($"Company name must not exceed {MaxLength.ExperienceCompanyName} characters.");

        RuleFor(x => x.createExperienceRequest.Position)
            .NotEmpty().WithMessage("Position is required.")
            .MaximumLength(MaxLength.ExperiencePosition).WithMessage($"Position must not exceed {MaxLength.ExperiencePosition} characters.");

        RuleFor(x => x.createExperienceRequest.Type)
            .NotEmpty().WithMessage("Type is required.")
            .IsInEnum().WithMessage("Type must be a valid enum value.");

        RuleFor(x => x.createExperienceRequest.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(MaxLength.ExperienceDescription).WithMessage($"Description must not exceed {MaxLength.ExperienceDescription} characters.");

        RuleFor(x => x.createExperienceRequest.StartDate)
            .NotEmpty().WithMessage("Start date is required.")
            .LessThanOrEqualTo(x => x.createExperienceRequest.EndDate ?? DateOnly.MaxValue)
            .WithMessage("Start date must be less than or equal to end date.");

        RuleFor(x => x.createExperienceRequest.EndDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("End date must be less than or equal to the current date.");
    }
}
