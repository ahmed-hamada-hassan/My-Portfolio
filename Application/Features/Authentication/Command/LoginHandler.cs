using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Errors;
using Domain.Shared;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Features.Authentication.Command;

internal sealed class LoginHandler
    : ICommandHandler<LoginCommand, Result>
{
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUserCommandRepository userCommandRepository,
        IPasswordHasher passwordHasher,
        IAuthenticationService authenticationService,
        ILogger<LoginHandler> logger)
    {
        _userCommandRepository = userCommandRepository;
        _passwordHasher = passwordHasher;
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.loginRequest;

        var user = await _userCommandRepository.GetByEmail(
            request.Email,
            cancellationToken);

        if (user is null)
        {
            _logger.LogWarning(
                "Login attempt failed due to invalid credentials.");

            return Result.Failure(
                UserErrors.InvalidCredentials);
        }

        var isValidPassword = _passwordHasher.Verify(
            user,
            user.HashedPassword,
            request.Password);

        if (!isValidPassword)
        {
            _logger.LogWarning(
                "Login attempt failed due to invalid credentials.");

            return Result.Failure(
                UserErrors.InvalidCredentials);
        }

        await _authenticationService.SignInAsync(
            user,
            cancellationToken);

        _logger.LogInformation(
            "User logged in successfully. UserId: {UserId}",
            user.Id);

        return Result.Success();
    }
}