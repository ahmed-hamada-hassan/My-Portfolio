using Application.Features.Project.DTOs;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;

namespace Application.Features.Project.Queries;

internal sealed class GetProjectBySlugHandler : IQueryHandler<GetProjectBySlugQuery, Result<ProjectDetailsResponse>>
{
    private readonly IProjectQueryRepository _projectQueryRepository;
    public GetProjectBySlugHandler(IProjectQueryRepository projectQueryRepository)
    {
        _projectQueryRepository = projectQueryRepository;
    }
    public async ValueTask<Result<ProjectDetailsResponse>> Handle(GetProjectBySlugQuery query, CancellationToken cancellationToken)
    {
        var result = await _projectQueryRepository.GetBySlugAsync(query.Slug, cancellationToken);

        if(result is null)
            return Result<ProjectDetailsResponse>.Failure(ProjectErrors.NotFound);

        return Result<ProjectDetailsResponse>.Success(result);
    }
}