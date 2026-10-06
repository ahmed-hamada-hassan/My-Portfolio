using Domain.Shared;
using Mediator;

namespace Application.Features.Experience.Command;

public sealed record MarkExperienceAsOngoingCommand(Guid ExperienceId) : ICommand<Result>;