using Domain.Shared;
using Mediator;

namespace Application.Features.Skill.Command;

public record DeleteSkillCommand(Guid SkillId) : ICommand<Result>;