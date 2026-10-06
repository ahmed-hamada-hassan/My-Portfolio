using Domain.Shared;
using Mediator;

namespace Application.Features.Project.Command;

public sealed record MarkProjectAsOngoingCommand(Guid ProjectId) : ICommand<Result>;