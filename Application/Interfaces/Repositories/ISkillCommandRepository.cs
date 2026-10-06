using Application.Interfaces.DI;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ISkillCommandRepository : IScopedMark
{
    Task<bool> AllExistAsync(IReadOnlyList<Guid> skillIds, CancellationToken ct = default);
    Task<Skill?> GetByIdAsync(Guid skillId, CancellationToken ct = default);
    Task AddAsync(Skill skill, CancellationToken ct = default);
    void Remove(Skill skill);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> IsUsedAsync(Guid skillId, CancellationToken ct = default);
}