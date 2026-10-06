using Application.Features.Project.DTOs;
using Application.Interfaces.Repositories;
using Application.Shared;
using Domain.Shared;
using Mediator;

namespace Application.Features.Project.Queries;

internal sealed class GetAllProjectsHandler : IQueryHandler<GetAllProjectsQuery, Result<PagedResult<ProjectListItemResponse>>>
{
    private readonly IProjectQueryRepository _projectQueryRepository;

    public GetAllProjectsHandler(IProjectQueryRepository projectQueryRepository)
    {
        _projectQueryRepository = projectQueryRepository;
    }

    public async ValueTask<Result<PagedResult<ProjectListItemResponse>>> Handle(GetAllProjectsQuery query, CancellationToken cancellationToken)
    {
        var result = await _projectQueryRepository
            .GetAllAsync(query.Page, query.PageSize, cancellationToken);

        return Result<PagedResult<ProjectListItemResponse>>.Success(result);
    }
}
