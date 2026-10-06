using Application.Features.Experience.Command;
using Application.Features.Experience.DTOs;
using Application.Features.Experience.Query;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[Route("api/experiences")]
[ApiController]
public class ExperiencesController : BaseController
{
    private readonly IMediator _mediator;

    public ExperiencesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting("PublicReading")]
    public async Task<ActionResult<IReadOnlyList<GetExperienceListItemDTO>>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetExperiencesQuery();
        var experiences = await _mediator.Send(query, cancellationToken);

        return experiences.IsFailure ? HandleFailure(experiences) : Ok(experiences.Value);
    }

    [HttpGet("{experienceId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminReading")]
    public async Task<ActionResult<GetExperienceDetailsDTO>> GetById(Guid experienceId, CancellationToken cancellationToken)
    {
        var query = new GetExperienceQuery(experienceId);
        var experience = await _mediator.Send(query, cancellationToken);

        return experience.IsFailure ? HandleFailure(experience) : Ok(experience.Value);
    }

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateExperienceDTO createExperienceRequest, CancellationToken cancellationToken)
    {
        var command = new CreateExperienceCommand(createExperienceRequest);
        var experienceId = await _mediator.Send(command, cancellationToken);

        return experienceId.IsFailure ? HandleFailure(experienceId) : 
            CreatedAtAction(nameof(GetById), new { experienceId = experienceId.Value }, experienceId.Value);
    }

    [HttpPatch("{experienceId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult> Update([FromRoute] Guid experienceId, [FromBody] UpdateExperienceDTO updateExperienceRequest,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExperienceCommand(experienceId, updateExperienceRequest);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpPatch("{experienceId:guid}/ongoing/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult> MarkAsOngoing([FromRoute] Guid experienceId, CancellationToken cancellationToken)
    {
        var command = new MarkExperienceAsOngoingCommand(experienceId);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{experienceId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult> Delete([FromRoute] Guid experienceId, CancellationToken cancellationToken)
    {
        var command = new DeleteExperienceCommand(experienceId);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}
