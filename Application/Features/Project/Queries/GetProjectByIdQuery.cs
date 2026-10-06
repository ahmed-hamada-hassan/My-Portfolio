using Application.Features.Project.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Project.Queries;

public record GetProjectByIdQuery(Guid ProjectId) : IQuery<Result<ProjectDetailsResponse>>;