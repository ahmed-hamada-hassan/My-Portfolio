using Application.Interfaces.Services;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Authentication.Command;

internal sealed class LogoutHandler
    : ICommandHandler<LogoutCommand, Result>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<LogoutHandler> _logger;

    public LogoutHandler(
        IAuthenticationService authenticationService,
        ILogger<LogoutHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        await _authenticationService.SignOutAsync(
            cancellationToken);

        _logger.LogInformation(
            "User logged out successfully.");

        return Result.Success();
    }
}