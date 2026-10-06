using Domain.Shared;
using Mediator;

namespace Application.Features.Experience.Command;

public sealed record DeleteExperienceCommand(Guid experienceId) : ICommand<Result>;