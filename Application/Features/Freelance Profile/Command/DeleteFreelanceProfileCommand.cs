using Domain.Shared;
using Mediator;

namespace Application.Features.Freelance_Profile.Command;

public sealed record DeleteFreelanceProfileCommand(
    Guid FreelanceProfileId)
    : ICommand<Result>;