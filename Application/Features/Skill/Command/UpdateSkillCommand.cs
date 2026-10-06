using Application.Features.Skill.DTO;
using Domain.Shared;
using Mediator;

namespace Application.Features.Skill.Command;

public record UpdateSkillCommand(Guid SkillId, UpdateSkillDTO UpdateSkillRequest) : ICommand<Result>;