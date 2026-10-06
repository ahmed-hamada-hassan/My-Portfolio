using Domain.Shared;
using Mediator;

namespace Application.Features.Project.Command;

public record DeleteProjectCommand(Guid ProjectId) : IRequest<Result>;