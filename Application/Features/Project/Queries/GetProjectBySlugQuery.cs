using Application.Features.Project.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Project.Queries;

public record GetProjectBySlugQuery(string Slug) : IQuery<Result<ProjectDetailsResponse>>;
