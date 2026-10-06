using Application.Features.Project.DTOs;
using Application.Interfaces.Repositories;
using Domain.Errors;
using Domain.Shared;
using Mediator;

namespace Application.Features.Project.Queries;

internal sealed class GetProjectByIdHandler : IQueryHandler<GetProjectByIdQuery, Result<ProjectDetailsResponse>>
{
    private readonly IProjectQueryRepository _projectQueryRepository;
    public GetProjectByIdHandler(IProjectQueryRepository projectQueryRepository)
    {
        _projectQueryRepository = projectQueryRepository;
    }
    public async ValueTask<Result<ProjectDetailsResponse>> Handle(GetProjectByIdQuery query, CancellationToken cancellationToken)
    {
        var result = await _projectQueryRepository.GetByIdAsync(query.ProjectId, cancellationToken);

        if(result is null)
            return Result<ProjectDetailsResponse>.Failure(ProjectErrors.NotFound);

        return Result<ProjectDetailsResponse>.Success(result);
    }
}
