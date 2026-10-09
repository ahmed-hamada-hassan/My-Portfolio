using Application.Features.CV.Query;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/cv")]
[ApiController]
public class CVController : BaseController
{
    private readonly IMediator _mediator;

    public CVController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("download")]
    [AllowAnonymous]
    public async Task<IActionResult> Download(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DownloadCVQuery(), cancellationToken);

        return result.IsFailure ? HandleFailure(result) : File(result.Value!, "application/pdf");
    }
}