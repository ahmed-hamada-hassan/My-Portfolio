using Application.Features.Skill.Command;
using Application.Features.Skill.DTO;
using Application.Features.Skill.Query;
using Domain.Enums;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[Route("api/skills")]
[ApiController]
public class SkillsController : BaseController
{
    private readonly IMediator _mediator;

    public SkillsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting("PublicReading")]
    public async Task<ActionResult<IReadOnlyList<SkillResponseDTO>>> GetAll([FromQuery] SkillType? type, CancellationToken ct)
    {
        var query = new GetAllSkillsQuery(type);
        var skills = await _mediator.Send(query, ct);

        return skills.IsFailure ? HandleFailure(skills) : Ok(skills.Value);
    }

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult<Guid>> CreateSkill([FromBody] CreateSkillDTO createSkillDTO, CancellationToken ct)
    {
        var command = new CreateSkillCommand(createSkillDTO);
        var skillId = await _mediator.Send(command, ct);

        return skillId.IsFailure ? HandleFailure(skillId) : StatusCode(StatusCodes.Status201Created, skillId.Value);
    }

    [HttpPatch("{skillId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult> UpdateSkill([FromRoute] Guid skillId, [FromBody] UpdateSkillDTO updateSkillDTO, CancellationToken ct)
    {
        var command = new UpdateSkillCommand(skillId, updateSkillDTO);
        var result = await _mediator.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{skillId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult> DeleteSkill([FromRoute] Guid skillId, CancellationToken ct)
    {
        var command = new DeleteSkillCommand(skillId);
        var result = await _mediator.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}
