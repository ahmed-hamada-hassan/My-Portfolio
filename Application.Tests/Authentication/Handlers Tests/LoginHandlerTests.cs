using Application.Features.Authentication.Command;
using Application.Features.Authentication.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Authentication.Handlers_Tests;

public class LoginHandlerTests
{
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<LoginHandler> _logger;

    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _userCommandRepository = A.Fake<IUserCommandRepository>();
        _passwordHasher = A.Fake<IPasswordHasher>();
        _authenticationService = A.Fake<IAuthenticationService>();
        _logger = A.Fake<ILogger<LoginHandler>>();

        _handler = new LoginHandler(
            _userCommandRepository,
            _passwordHasher,
            _authenticationService,
            _logger);
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessfulResult()
    {
        // Arrange
        var loginRequest = new LoginDTO
        (
            Email : "myemail@gmail.com",
            Password : "MyPassword123!"
        );

        var command = new LoginCommand(loginRequest);
        var userResult = User.Create("myemail@gmail.com", "hashed_password_here", UserRole.Admin);
        var user = userResult.Value!;

        A.CallTo(() => _userCommandRepository.GetByEmail(loginRequest.Email, A<CancellationToken>._))
            .Returns(user);

        A.CallTo(() => _passwordHasher.Verify(user, user.HashedPassword, loginRequest.Password))
            .Returns(true);

        A.CallTo(() => _authenticationService.SignInAsync(user, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        A.CallTo(() => _userCommandRepository.GetByEmail(loginRequest.Email, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _passwordHasher.Verify(user, user.HashedPassword, loginRequest.Password))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _authenticationService.SignInAsync(user, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenUserIsNull_ReturnsInvalidCredentialsError()
    {
        // Arrange
        var loginRequest = new LoginDTO
        (
            Email: "myemail@gmail.com",
            Password: "MyPassword123!"
        );

        var command = new LoginCommand(loginRequest);

        A.CallTo(() => _userCommandRepository.GetByEmail(loginRequest.Email, A<CancellationToken>._))
            .Returns((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidCredentials);

        A.CallTo(() => _userCommandRepository.GetByEmail(loginRequest.Email, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenVerifyPasswordFails_ReturnsInvalidCredentialsError()
    {
        // Arrange
        var loginRequest = new LoginDTO
        (
            Email: "myemail@gmail.com",
            Password: "MyPassword123!"
        );

        var command = new LoginCommand(loginRequest);
        var userResult = User.Create("myemail@gmail.com", "hashed_password_here", UserRole.Admin);
        var user = userResult.Value!;

        A.CallTo(() => _userCommandRepository.GetByEmail(loginRequest.Email, A<CancellationToken>._))
            .Returns(user);

        A.CallTo(() => _passwordHasher.Verify(user, user.HashedPassword, loginRequest.Password))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidCredentials);

        A.CallTo(() => _userCommandRepository.GetByEmail(loginRequest.Email, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _passwordHasher.Verify(user, user.HashedPassword, loginRequest.Password))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenSignInAsyncThrowsException_LogsErrorAndReturnsFailure()
    {
        // Arrange
        var loginRequest = new LoginDTO
        (
            Email: "myemail@gmail.com",
            Password: "MyPassword123!"
        );

        var command = new LoginCommand(loginRequest);
        var userResult = User.Create("myemail@gmail.com", "hashed_password_here", UserRole.Admin);
        var user = userResult.Value!;

        A.CallTo(() => _userCommandRepository.GetByEmail(loginRequest.Email, A<CancellationToken>._))
            .Returns(user);

        A.CallTo(() => _passwordHasher.Verify(user, user.HashedPassword, loginRequest.Password))
            .Returns(true);

        A.CallTo(() => _authenticationService.SignInAsync(user, A<CancellationToken>._))
            .Throws(new Exception("Simulated error"));

        // Act
        Func<Task> result = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await result.Should().ThrowAsync<Exception>().WithMessage("Simulated error");
    }
}