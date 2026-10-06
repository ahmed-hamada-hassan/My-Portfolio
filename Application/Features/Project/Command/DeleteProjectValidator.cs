using FluentValidation;

namespace Application.Features.Project.Command;

internal sealed class DeleteProjectValidator : AbstractValidator<DeleteProjectCommand>
{
    public DeleteProjectValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Project ID cannot be empty.");
    }
}
