using Application.Features.Authentication.DTOs;
using Domain.Shared;
using Mediator;

namespace Application.Features.Authentication.Command;

public sealed record LoginCommand(LoginDTO loginRequest) : ICommand<Result>;