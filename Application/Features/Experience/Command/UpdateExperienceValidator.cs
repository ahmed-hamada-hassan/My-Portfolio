using FluentValidation;

namespace Application.Features.Experience.Command;

internal sealed class UpdateExperienceValidator : AbstractValidator<UpdateExperienceCommand>
{
    public UpdateExperienceValidator()
    {
        RuleFor(x => x.experienceId)
            .NotEmpty().WithMessage("Experience ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Experience ID cannot be empty.");

        RuleFor(x => x.updateExperienceRequest.CompanyName)
            .MaximumLength(Domain.Constants.MaxLength.ExperienceCompanyName)
            .WithMessage($"Company name must not exceed {Domain.Constants.MaxLength.ExperienceCompanyName} characters.");

        RuleFor(x => x.updateExperienceRequest.Position)
            .MaximumLength(Domain.Constants.MaxLength.ExperiencePosition)
            .WithMessage($"Position must not exceed {Domain.Constants.MaxLength.ExperiencePosition} characters.");

        RuleFor(x => x.updateExperienceRequest.Type)
            .IsInEnum().WithMessage("Type must be a valid enum value.");

        RuleFor(x => x.updateExperienceRequest.Description)
            .MaximumLength(Domain.Constants.MaxLength.ExperienceDescription)
            .WithMessage($"Description must not exceed {Domain.Constants.MaxLength.ExperienceDescription} characters.");

        RuleFor(x => x.updateExperienceRequest.StartDate)
            .LessThanOrEqualTo(x => x.updateExperienceRequest.EndDate ?? DateOnly.MaxValue)
            .WithMessage("Start date must be less than or equal to end date.");

        RuleFor(x => x.updateExperienceRequest.EndDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("End date must be less than or equal to the current date.");
    }
}
