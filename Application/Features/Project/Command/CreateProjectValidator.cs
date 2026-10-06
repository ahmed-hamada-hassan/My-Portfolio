using Application.Features.Project_Images.Command;
using Application.Features.Project_Images.DTOs;
using Domain.Constants;
using FluentValidation;

namespace Application.Features.Project.Command;

internal sealed class CreateProjectValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.CreateProjectRequest.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(MaxLength.ProjectTitle).WithMessage($"Title must not exceed {MaxLength.ProjectTitle} characters.");

        RuleFor(x => x.CreateProjectRequest.Description)
            .MaximumLength(MaxLength.ProjectDescription).WithMessage($"Description must not exceed {MaxLength.ProjectDescription} characters.");

        RuleFor(x => x.CreateProjectRequest.StartDate)
            .LessThanOrEqualTo(x => x.CreateProjectRequest.EndDate)
            .When(x => x.CreateProjectRequest.EndDate.HasValue)
            .WithMessage("Start date must be less than or equal to end date.");

        RuleFor(x => x.CreateProjectRequest.LiveURL)
            .MaximumLength(MaxLength.ProjectLiveURL).WithMessage($"Live URL must not exceed {MaxLength.ProjectLiveURL} characters.")
            .When(x => !string.IsNullOrEmpty(x.CreateProjectRequest.LiveURL));

        RuleFor(x => x.CreateProjectRequest.GitHubURL)
            .NotEmpty().WithMessage("GitHub URL is required.")
            .MaximumLength(MaxLength.ProjectGitHubURL).WithMessage($"GitHub URL must not exceed {MaxLength.ProjectGitHubURL} characters.")
            .When(x => !string.IsNullOrEmpty(x.CreateProjectRequest.GitHubURL));

        RuleFor(x => x.CreateProjectRequest.ProjectImages)
            .Must(HaveOnlyOneCoverImage)
            .WithMessage("Only one project image can be the cover image.");

        RuleFor(x => x.CreateProjectRequest.ProjectImages)
            .Must(HaveUniqueDisplayOrders)
            .WithMessage("Project image display orders must be unique.");

        RuleForEach(x => x.CreateProjectRequest.ProjectImages)
            .SetValidator(new AddProjectImagesValidator());

        RuleForEach(x => x.CreateProjectRequest.SkillIds)
            .NotEmpty().WithMessage("Skill ID cannot be empty.")
            .NotEqual(Guid.Empty).WithMessage("Skill ID cannot be an empty GUID.");

        RuleFor(x => x.CreateProjectRequest.SkillIds)
            .Must(HaveUniqueSkillIds)
            .WithMessage("Skill IDs must be unique.");
    }

    private static bool HaveOnlyOneCoverImage(
    IReadOnlyList<AddProjectImageDTO> images)
    {
        return images.Count(image => image.IsCover) == 1;
    }

    private static bool HaveUniqueDisplayOrders(
        IReadOnlyList<AddProjectImageDTO> images)
    {
        return images
            .Select(image => image.DisplayOrder)
            .Distinct()
            .Count() == images.Count();
    }

    private static bool HaveUniqueSkillIds(IReadOnlyList<Guid> skillIds)
    {
        return skillIds.Distinct().Count() == skillIds.Count();
    }
}