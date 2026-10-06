using Application.Features.Project.DTOs;
using Application.Interfaces.DI;
using Application.Shared;

namespace Application.Interfaces.Repositories;

public interface IProjectQueryRepository : IScopedMark
{
    Task<PagedResult<ProjectListItemResponse>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ProjectDetailsResponse?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<ProjectDetailsResponse?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
