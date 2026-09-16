using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.EF_Core.Repositories;

internal sealed class ProjectCommandRepository : IProjectCommandRepository
{
    private readonly AppDbContext _dbContext;

    public ProjectCommandRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await _dbContext.Projects.AddAsync(project, cancellationToken);
    }

    public void Update(Project project)
    {
        _dbContext.Projects.Update(project);
    }

    public void Remove(Project project)
    {
        _dbContext.Projects.Remove(project);
    }

    public Task<bool> TitleExistsAsync(string title, CancellationToken cancellationToken = default)
    {
        return _dbContext.Projects.AnyAsync(p => p.Title == title, cancellationToken);
    }
}
