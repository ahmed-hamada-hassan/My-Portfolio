using Application.Features.Project_Images.DTOs;
using Application.Features.Project_Images.Validators;
using Domain.Constants;
using FluentValidation;

namespace Application.Features.Project.Command;

internal sealed class CreateProjectValidator
    : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectValidator(
        AddProjectImageDTOValidator imageValidator)
    {
        RuleFor(x => x.CreateProjectRequest.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(MaxLength.ProjectTitle)
            .WithMessage(
                $"Title must not exceed {MaxLength.ProjectTitle} characters.");

        RuleFor(x => x.CreateProjectRequest.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(MaxLength.ProjectDescription)
            .WithMessage(
                $"Description must not exceed {MaxLength.ProjectDescription} characters.");

        RuleFor(x => x.CreateProjectRequest.StartDate)
            .LessThanOrEqualTo(x => x.CreateProjectRequest.EndDate!.Value)
            .When(x => x.CreateProjectRequest.EndDate.HasValue)
            .WithMessage("Start date must be less than or equal to end date.");

        RuleFor(x => x.CreateProjectRequest.LiveURL)
            .MaximumLength(MaxLength.ProjectLiveURL)
            .WithMessage(
                $"Live URL must not exceed {MaxLength.ProjectLiveURL} characters.")
            .When(x => !string.IsNullOrWhiteSpace(
                x.CreateProjectRequest.LiveURL));

        RuleFor(x => x.CreateProjectRequest.GitHubURL)
            .NotEmpty()
            .WithMessage("GitHub URL is required.")
            .MaximumLength(MaxLength.ProjectGitHubURL)
            .WithMessage(
                $"GitHub URL must not exceed {MaxLength.ProjectGitHubURL} characters.");

        RuleFor(x => x.CreateProjectRequest.ProjectImages)
            .NotNull()
            .WithMessage("Project images are required.")
            .Must(images => images!.Count > 0)
            .WithMessage("At least one project image is required.")
            .Must(HaveOnlyOneCoverImage)
            .WithMessage("Exactly one project image must be the cover image.")
            .Must(HaveUniqueDisplayOrders)
            .WithMessage("Project image display orders must be unique.");

        RuleForEach(x => x.CreateProjectRequest.ProjectImages)
            .SetValidator(imageValidator);

        RuleFor(x => x.CreateProjectRequest.SkillIds)
            .NotNull()
            .WithMessage("Skill IDs are required.")
            .Must(HaveUniqueSkillIds)
            .WithMessage("Skill IDs must be unique.");

        RuleForEach(x => x.CreateProjectRequest.SkillIds)
            .NotEqual(Guid.Empty)
            .WithMessage("Skill ID cannot be an empty GUID.");
    }

    private static bool HaveOnlyOneCoverImage(
        IReadOnlyList<AddProjectImageDTO>? images)
    {
        return images is not null &&
               images.Count(image => image.IsCover) == 1;
    }

    private static bool HaveUniqueDisplayOrders(
        IReadOnlyList<AddProjectImageDTO>? images)
    {
        return images is not null &&
               images.Select(image => image.DisplayOrder)
                   .Distinct()
                   .Count() == images.Count;
    }

    private static bool HaveUniqueSkillIds(
        IReadOnlyList<Guid>? skillIds)
    {
        return skillIds is not null &&
               skillIds.Distinct().Count() == skillIds.Count;
    }
}