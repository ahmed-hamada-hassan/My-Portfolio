using Domain.Shared;
using Mediator;

namespace Application.Features.Authentication.Command;

public sealed record LogoutCommand : ICommand<Result>;