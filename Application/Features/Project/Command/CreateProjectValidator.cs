using FluentValidation;

namespace Application.Features.Project.Command;

internal sealed class CreateProjectValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.CreateProjectRequest.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

        RuleFor(x => x.CreateProjectRequest.Description)
            .MaximumLength(1500).WithMessage("Description must not exceed 1500 characters.");

        RuleFor(x => x.CreateProjectRequest.StartDate)
            .LessThanOrEqualTo(x => x.CreateProjectRequest.EndDate)
            .When(x => x.CreateProjectRequest.EndDate.HasValue)
            .WithMessage("Start date must be less than or equal to end date.");
    }
}
