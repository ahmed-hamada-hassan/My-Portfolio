using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.EF_Core.Repositories;

internal sealed class SkillCommandRepository : ISkillCommandRepository
{
    private readonly AppDbContext _dbContext;

    public SkillCommandRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> AllExistAsync(IReadOnlyList<Guid> skillIds, CancellationToken ct = default)
    {
        var distinctSkillIds = skillIds.Distinct().ToList();

        var count = await _dbContext.Skills
            .CountAsync(s => distinctSkillIds.Contains(s.Id), ct);

        return count == skillIds.Count;
    }

    public Task<Skill?> GetByIdAsync(Guid skillId, CancellationToken ct = default)
    {
        return _dbContext.Skills.FirstOrDefaultAsync(s => s.Id == skillId, ct);
    }

    public Task AddAsync(Skill skill, CancellationToken ct = default)
    {
        return _dbContext.Skills.AddAsync(skill, ct).AsTask();
    }

    public void Remove(Skill skill)
    {
        _dbContext.Skills.Remove(skill);
    }

    public Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        return _dbContext.Skills
            .AnyAsync(s => s.Name == name && (excludeId.HasValue || s.Id != excludeId!.Value), ct);
    }

    public Task<bool> IsUsedAsync(Guid skillId, CancellationToken ct = default)
    {
        return _dbContext.ProjectSkills.AnyAsync(ps => ps.SkillId == skillId, ct);
    }
}
