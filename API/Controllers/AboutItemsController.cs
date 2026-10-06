using Application.Features.About_Item.Command;
using Application.Features.About_Item.DTOs;
using Application.Features.About_Item.Query;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[Route("api/about-items")]
[ApiController]
public class AboutItemsController : BaseController
{
    private readonly IMediator _mediator;

    public AboutItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting("PublicReading")]
    public async Task<ActionResult<IReadOnlyList<GetAboutItemDTO>>> GetAboutItems(CancellationToken cancellationToken)
    {
        var query = new GetAboutItemsQuery();

        var result = await _mediator.Send(
            query,
            cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminReading")]
    public async Task<ActionResult<IReadOnlyList<GetDetailsAboutItemDTO>>> GetDetailedAboutItems(CancellationToken cancellationToken)
    {
        var query = new GetDetailedAboutItemsQuery();

        var result = await _mediator.Send(
            query,
            cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet("{aboutItemId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminReading")]
    public async Task<ActionResult<GetDetailsAboutItemDTO>> GetDetailedAboutItemById([FromRoute] Guid aboutItemId, CancellationToken cancellationToken)
    {
        var query = new GetDetailedAboutItemQuery(
            aboutItemId);

        var result = await _mediator.Send(
            query,
            cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult<Guid>> CreateAboutItem([FromBody] CreateAboutItemDTO request, CancellationToken cancellationToken)
    {
        var command = new CreateAboutItemCommand(request);

        var result = await _mediator.Send(
            command,
            cancellationToken);

        return result.IsFailure ? HandleFailure(result) : CreatedAtAction(
            nameof(GetDetailedAboutItemById),
            new { aboutItemId = result.Value },
            result.Value);
    }

    [HttpPatch("{aboutItemId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<IActionResult> UpdateAboutItem([FromRoute] Guid aboutItemId, [FromBody] UpdateAboutItemDTO request, CancellationToken cancellationToken)
    {
        var command = new UpdateAboutItemCommand(
            aboutItemId,
            request);

        var result = await _mediator.Send(
            command,
            cancellationToken);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{aboutItemId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<IActionResult> DeleteAboutItem([FromRoute] Guid aboutItemId, CancellationToken cancellationToken)
    {
        var command = new DeleteAboutItemCommand(
            aboutItemId);

        var result = await _mediator.Send(
            command,
            cancellationToken);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}