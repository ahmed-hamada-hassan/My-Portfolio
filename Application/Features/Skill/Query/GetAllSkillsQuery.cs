using Application.Features.Skill.DTO;
using Domain.Enums;
using Domain.Shared;
using Mediator;

namespace Application.Features.Skill.Query;

public sealed record GetAllSkillsQuery(SkillType? Type) : IQuery<Result<IReadOnlyList<SkillResponseDTO>>>;