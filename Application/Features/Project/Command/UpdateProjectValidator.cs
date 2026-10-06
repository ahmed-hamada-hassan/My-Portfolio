using Domain.Constants;
using FluentValidation;

namespace Application.Features.Project.Command;

internal sealed class UpdateProjectValidator
    : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.")
            .NotEqual(Guid.Empty)
            .WithMessage("Project ID cannot be empty.");

        RuleFor(x => x.UpdateProjectRequest.Title)
            .MaximumLength(MaxLength.ProjectTitle)
            .WithMessage(
                $"Title must not exceed {MaxLength.ProjectTitle} characters.");

        RuleFor(x => x.UpdateProjectRequest.Description)
            .MaximumLength(MaxLength.ProjectDescription)
            .WithMessage(
                $"Description must not exceed {MaxLength.ProjectDescription} characters.");

        RuleFor(x => x.UpdateProjectRequest.GitHubURL)
            .MaximumLength(MaxLength.ProjectGitHubURL)
            .WithMessage(
                $"GitHub URL must not exceed {MaxLength.ProjectGitHubURL} characters.");

        RuleFor(x => x.UpdateProjectRequest.LiveURL)
            .MaximumLength(MaxLength.ProjectLiveURL)
            .WithMessage(
                $"Live URL must not exceed {MaxLength.ProjectLiveURL} characters.");

        RuleFor(x => x.UpdateProjectRequest)
            .Must(request =>
                request.Title is not null ||
                request.Description is not null ||
                request.StartDate.HasValue ||
                request.EndDate.HasValue ||
                request.LiveURL is not null ||
                request.GitHubURL is not null)
            .WithMessage("At least one field must be provided.");
    }
}