using Application.Interfaces.DI;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IExperienceCommandRepository : IScopedMark
{
    Task<Experience?> GetByIdAsync(Guid ExperienceId, CancellationToken cancellationToken = default);
    Task AddAsync(Experience experience, CancellationToken cancellationToken = default);
    void Remove(Experience experience);
}
