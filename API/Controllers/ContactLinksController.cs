using Application.Features.Contact_Links.Command;
using Application.Features.Contact_Links.DTOs;
using Application.Features.Contact_Links.Query;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[Route("api/contact-links")]
[ApiController]
public class ContactLinksController : BaseController
{
    private readonly IMediator _mediator;

    public ContactLinksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting("PublicReading")]
    public async Task<ActionResult<IReadOnlyList<GetContactLinkDTO>>> GetSocialLinks(
        CancellationToken cancellationToken)
    {
        var query = new GetContactLinksQuery();

        var result = await _mediator.Send(query, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminReading")]
    public async Task<ActionResult<IReadOnlyList<GetDetailsContactLinkDTO>>> GetDetailedSocialLinks(
        CancellationToken cancellationToken)
    {
        var query = new GetDetailedContactLinksQuery();

        var result = await _mediator.Send(query, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet("{socialLinkId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminReading")]
    public async Task<ActionResult<GetDetailsContactLinkDTO>> GetDetailedSocialLinkById(
        [FromRoute] Guid socialLinkId,
        CancellationToken cancellationToken)
    {
        var query = new GetDetailedContactLinkQuery(socialLinkId);

        var result = await _mediator.Send(query, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult<Guid>> CreateSocialLink(
        [FromBody] CreateContactLinkDTO createSocialLinkRequest,
        CancellationToken cancellationToken)
    {
        var command = new CreateContactLinkCommand(createSocialLinkRequest);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : CreatedAtAction(
            nameof(GetDetailedSocialLinkById),
            new { socialLinkId = result.Value },
            result.Value);
    }

    [HttpPatch("{socialLinkId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<IActionResult> UpdateSocialLink(
        [FromRoute] Guid socialLinkId,
        [FromBody] UpdateContactLinkDTO updateSocialLinkRequest,
        CancellationToken cancellationToken)
    {
        var command = new UpdateContactLinkCommand(
            socialLinkId,
            updateSocialLinkRequest);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{socialLinkId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<IActionResult> DeleteSocialLink(
        [FromRoute] Guid socialLinkId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteContactLinkCommand(socialLinkId);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}