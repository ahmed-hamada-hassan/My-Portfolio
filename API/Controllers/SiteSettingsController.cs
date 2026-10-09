using Application.Features.Site_Settings.Command;
using Application.Features.Site_Settings.Query;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[Route("api/site-settings")]
[ApiController]
[EnableRateLimiting("AdminManagement")]
public sealed class SiteSettingsController : BaseController
{
    private readonly IMediator _mediator;

    public SiteSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetSiteSettings(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSiteSettingsQuery(), cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPatch("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSiteSettings([FromBody] UpdateSiteSettingsCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpPut("profile-image/admin")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProfileImage([FromForm] UpdateProfileImageCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpPut("cv/admin")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateCV([FromForm] UpdateCVCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}
