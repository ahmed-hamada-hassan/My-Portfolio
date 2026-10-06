using Domain.Constants;
using FluentValidation;

namespace Application.Features.Skill.Command;

internal sealed class CreateSkillValidator : AbstractValidator<CreateSkillCommand>
{
    public CreateSkillValidator()
    {
        RuleFor(x => x.CreateSkillRequest.Name)
            .NotEmpty().WithMessage("Skill name is required.")
            .MaximumLength(MaxLength.SkillName).WithMessage($"Skill name must not exceed {MaxLength.SkillName} characters.");

        RuleFor(x => x.CreateSkillRequest.Type)
            .IsInEnum().WithMessage("Invalid skill type.");

        RuleFor(x => x.CreateSkillRequest.Icon)
            .NotEmpty().WithMessage("Skill icon is required.")
            .MaximumLength(MaxLength.SkillIcon).WithMessage($"Skill icon must not exceed {MaxLength.SkillIcon} characters.");
    }
}
