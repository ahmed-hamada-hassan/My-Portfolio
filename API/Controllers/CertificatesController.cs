using Application.Features.Certificate.Command;
using Application.Features.Certificate.DTOs;
using Application.Features.Certificate.Query;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[Route("api/certificates")]
[ApiController]
public class CertificatesController : BaseController
{
    private readonly IMediator _mediator;

    public CertificatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting("PublicReading")]
    public async Task<ActionResult<IReadOnlyList<GetCertificateDTO>>> GetAll(CancellationToken ct)
    {
        var query = new GetCertificatesQuery();
        var certificates = await _mediator.Send(query, ct);

        return certificates.IsFailure ? HandleFailure(certificates) : Ok(certificates.Value);
    }

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult<Guid>> Create([FromForm] CreateCertificateDTO createCertificateDTO, CancellationToken ct)
    {
        var command = new CreateCertificateCommand(createCertificateDTO);
        var certificateId = await _mediator.Send(command, ct);

        return certificateId.IsFailure ? HandleFailure(certificateId) : StatusCode(StatusCodes.Status201Created, certificateId.Value);
    }

    [HttpPatch("{certificateId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult> Update([FromRoute] Guid certificateId, [FromBody] UpdateCertificateDTO updateCertificateDTO, CancellationToken ct)
    {
        var command = new UpdateCertificateCommand(certificateId, updateCertificateDTO);
        var result = await _mediator.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpPatch("{certificateId:guid}/image/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult> UpdateImage([FromRoute] Guid certificateId, [FromForm] IFormFile image, CancellationToken ct)
    {
        var command = new UpdateCertificateImageCommand(certificateId, image);
        var result = await _mediator.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{certificateId:guid}/admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("AdminManagement")]
    public async Task<ActionResult> Delete([FromRoute] Guid certificateId, CancellationToken ct)
    {
        var command = new DeleteCertificateCommand(certificateId);
        var result = await _mediator.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}
