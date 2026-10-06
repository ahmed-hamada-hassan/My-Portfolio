using Application.Features.Authentication.Command;
using Application.Features.Authentication.DTOs;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : BaseController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request);

        var result = await _mediator.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
            return HandleFailure(result);

        return NoContent();
    }

    [HttpPost("logout")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var command = new LogoutCommand();

        var result = await _mediator.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
            return HandleFailure(result);

        return NoContent();
    }
}