using Application.Features.Project.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Project.Command;

internal sealed record CreateProjectCommand(CreateProjectDTO CreateProjectRequest) : ICommand<Result<Guid>>;