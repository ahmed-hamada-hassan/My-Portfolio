using Application.Features.Skill.Command;
using Application.Features.Skill.DTO;
using Domain.Constants;
using FluentValidation;

internal sealed class UpdateSkillValidator
    : AbstractValidator<UpdateSkillCommand>
{
    public UpdateSkillValidator()
    {
        RuleFor(x => x.SkillId)
            .NotEmpty()
            .WithMessage("SkillId is required.");

        RuleFor(x => x.UpdateSkillRequest)
            .Must(HasAtLeastOneValue)
            .WithMessage("At least one field must be provided.");

        RuleFor(x => x.UpdateSkillRequest.Name)
            .MaximumLength(MaxLength.SkillName)
            .When(x => x.UpdateSkillRequest.Name is not null)
            .WithMessage(
                $"Skill name must not exceed {MaxLength.SkillName} characters.");

        RuleFor(x => x.UpdateSkillRequest.Icon)
            .MaximumLength(MaxLength.SkillIcon)
            .When(x => x.UpdateSkillRequest.Icon is not null)
            .WithMessage(
                $"Skill icon must not exceed {MaxLength.SkillIcon} characters.");

        RuleFor(x => x.UpdateSkillRequest.Type)
            .IsInEnum()
            .When(x => x.UpdateSkillRequest.Type.HasValue)
            .WithMessage("Invalid skill type.");
    }

    private static bool HasAtLeastOneValue(UpdateSkillDTO request)
    {
        return request.Name is not null
            || request.Type.HasValue
            || request.Icon is not null;
    }
}