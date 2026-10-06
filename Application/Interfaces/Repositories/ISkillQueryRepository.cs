using Application.Features.Skill.DTO;
using Application.Interfaces.DI;
using Domain.Enums;

namespace Application.Interfaces.Repositories;

public interface ISkillQueryRepository : IScopedMark
{
    Task<IReadOnlyList<SkillResponseDTO>> GetAllAsync(SkillType? Type = null, CancellationToken cancellationToken = default);
}