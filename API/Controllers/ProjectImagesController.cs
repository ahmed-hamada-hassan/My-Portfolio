using Application.Features.Project_Images.Command;
using Application.Features.Project_Images.DTOs;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[Route("api/projects")]
[ApiController]
public class ProjectImagesController : BaseController
{
    private readonly IMediator _mediator;

    public ProjectImagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{projectId:guid}/images/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult> AddProjectImages([FromRoute] Guid projectId, [FromForm] IReadOnlyList<AddProjectImageDTO> addProjectImageRequest, CancellationToken ct)
    {
        var command = new AddProjectImagesCommand(projectId, addProjectImageRequest);

        var result = await _mediator.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpPut("{projectId:guid}/images/{imageId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult> UpdateProjectImage([FromRoute] Guid projectId, [FromRoute] Guid imageId,
        [FromForm] UpdateProjectImageDTO updateProjectImageRequest, CancellationToken ct)
    {
        var command = new UpdateProjectImageCommand(projectId, imageId, updateProjectImageRequest);
        var result = await _mediator.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{projectId:guid}/images/{imageId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult> DeleteProjectImage([FromRoute] Guid projectId, [FromRoute] Guid imageId, CancellationToken ct)
    {
        var command = new DeleteProjectImageCommand(projectId, imageId);
        var result = await _mediator.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}
