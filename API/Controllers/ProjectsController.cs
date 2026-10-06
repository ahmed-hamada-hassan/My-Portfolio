using Application.Features.Project.Command;
using Application.Features.Project.DTOs;
using Application.Features.Project.Queries;
using Application.Shared;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[Route("api/projects")]
[ApiController]
public class ProjectsController : BaseController
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting("PublicReading")]
    public async Task<ActionResult<PagedResult<ProjectListItemResponse>>> GetProjects([FromQuery] PaginationRequest paginationRequest, 
        CancellationToken ct = default)
    {
        var query = new GetAllProjectsQuery(paginationRequest.Page, paginationRequest.PageSize);
        var projects = await _mediator.Send(query, ct);

        return projects.IsFailure ? HandleFailure(projects) : Ok(projects.Value);
    }

    [HttpGet("{slug}")]
    [EnableRateLimiting("PublicReading")]
    public async Task<ActionResult<ProjectDetailsResponse>> GetProjectBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var query = new GetProjectBySlugQuery(slug);
        var project = await _mediator.Send(query, ct);

        return project.IsFailure ? HandleFailure(project) : Ok(project.Value);
    }

    [HttpGet("{projectId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminReading")]
    public async Task<ActionResult<ProjectDetailsResponse>> GetProjectById([FromRoute] Guid projectId, CancellationToken ct)
    {
        var query = new GetProjectByIdQuery(projectId);
        var project = await _mediator.Send(query, ct);

        return project.IsFailure ? HandleFailure(project) : Ok(project.Value);
    }

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult<Guid>> CreateProject([FromForm] CreateProjectDTO createProjectDTO, CancellationToken ct)
    {
        var command = new CreateProjectCommand(createProjectDTO);
        var projectId = await _mediator.Send(command, ct);

        return projectId.IsFailure ? HandleFailure(projectId) : 
            CreatedAtAction(nameof(GetProjectById), new { projectId = projectId.Value }, projectId.Value);
    }

    [HttpPut("{projectId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult> UpdateProject([FromRoute] Guid projectId, [FromBody] UpdateProjectDTO updateProjectDTO, CancellationToken ct)
    {
        var command = new UpdateProjectCommand(projectId, updateProjectDTO);
        var result = await _mediator.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{projectId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult> DeleteProject(Guid projectId, CancellationToken ct)
    {
        var command = new DeleteProjectCommand(projectId);
        var result = await _mediator.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpPatch("{projectId:guid}/ongoing/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult> UpdateProjectOngoingStatus([FromRoute] Guid projectId, CancellationToken ct)
    {
        var command = new MarkProjectAsOngoingCommand(projectId);
        var result = await _mediator.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}