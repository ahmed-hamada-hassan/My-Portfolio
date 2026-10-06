using Application.Features.Project.DTOs;
using Application.Shared;
using Domain.Shared;
using Mediator;

namespace Application.Features.Project.Queries;

public record GetAllProjectsQuery(int Page = 1, int PageSize = 10) : IQuery<Result<PagedResult<ProjectListItemResponse>>>;