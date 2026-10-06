using Application.Features.Skill.DTO;
using Domain.Shared;
using Mediator;

namespace Application.Features.Skill.Command;

public record CreateSkillCommand(CreateSkillDTO CreateSkillRequest) : ICommand<Result<Guid>>;