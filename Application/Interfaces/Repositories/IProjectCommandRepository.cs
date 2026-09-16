using Application.Interfaces.DI;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IProjectCommandRepository : IScopedMark
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    void Update(Project project);
    void Remove(Project project);
    Task<bool> TitleExistsAsync(string title, CancellationToken cancellationToken = default);
}