using Application.Features.Freelance_Profile.Command;
using Application.Features.Freelance_Profile.DTOs;
using Application.Features.Freelance_Profile.Query;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[Route("api/freelance-profiles")]
[ApiController]
public class FreelanceProfilesController : BaseController
{
    private readonly IMediator _mediator;

    public FreelanceProfilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting("PublicReading")]
    public async Task<ActionResult<IReadOnlyList<GetFreelanceProfileDTO>>> GetFreelanceProfiles(
        CancellationToken cancellationToken)
    {
        var query = new GetFreelanceProfilesQuery();

        var result = await _mediator.Send(query, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminReading")]
    public async Task<ActionResult<IReadOnlyList<GetDetailsFreelanceProfileDTO>>> GetDetailedFreelanceProfiles(
        CancellationToken cancellationToken)
    {
        var query = new GetDetailedFreelanceProfilesQuery();

        var result = await _mediator.Send(query, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet("{freelanceProfileId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminReading")]
    public async Task<ActionResult<GetDetailsFreelanceProfileDTO>> GetDetailedFreelanceProfileById(
        [FromRoute] Guid freelanceProfileId,
        CancellationToken cancellationToken)
    {
        var query = new GetDetailedFreelanceProfileQuery(
            freelanceProfileId);

        var result = await _mediator.Send(query, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult<Guid>> CreateFreelanceProfile(
        [FromBody] CreateFreelanceProfileDTO createFreelanceProfileRequest,
        CancellationToken cancellationToken)
    {
        var command = new CreateFreelanceProfileCommand(
            createFreelanceProfileRequest);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : CreatedAtAction(
            nameof(GetDetailedFreelanceProfileById),
            new { freelanceProfileId = result.Value },
            result.Value);
    }

    [HttpPatch("{freelanceProfileId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<IActionResult> UpdateFreelanceProfile(
        [FromRoute] Guid freelanceProfileId,
        [FromBody] UpdateFreelanceProfileDTO updateFreelanceProfileRequest,
        CancellationToken cancellationToken)
    {
        var command = new UpdateFreelanceProfileCommand(
            freelanceProfileId,
            updateFreelanceProfileRequest);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{freelanceProfileId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<IActionResult> DeleteFreelanceProfile(
        [FromRoute] Guid freelanceProfileId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteFreelanceProfileCommand(
            freelanceProfileId);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}