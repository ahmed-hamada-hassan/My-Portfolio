using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.EF_Core.Repositories;

internal sealed class ExperienceCommandRepository : IExperienceCommandRepository
{
    private readonly AppDbContext _dbContext;

    public ExperienceCommandRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Experience experience, CancellationToken cancellationToken = default)
    {
        await _dbContext.Experiences.AddAsync(experience, cancellationToken);
    }

    public async Task<Experience?> GetByIdAsync(Guid ExperienceId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Experiences.FirstOrDefaultAsync(e => e.Id == ExperienceId, cancellationToken);
    }

    public void Remove(Experience experience)
    {
        _dbContext.Experiences.Remove(experience);
    }
}
