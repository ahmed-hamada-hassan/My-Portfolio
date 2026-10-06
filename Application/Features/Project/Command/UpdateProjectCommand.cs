using Application.Features.Project.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Project.Command;

public record UpdateProjectCommand(Guid ProjectId, UpdateProjectDTO UpdateProjectRequest) : IRequest<Result>;