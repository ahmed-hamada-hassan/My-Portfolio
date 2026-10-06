using Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public abstract class BaseController : ControllerBase
{
    protected ActionResult HandleFailure<T>(Result<T> result)
    {
        return CreateErrorResponse(result.Error!);
    }
    protected ActionResult HandleFailure(Result result)
    {
        return CreateErrorResponse(result.Error!);
    }

    protected ActionResult CreateErrorResponse(Error error)
    {
        if (error.Code.EndsWith("NotFound", StringComparison.OrdinalIgnoreCase))
            return NotFound(error);

        if (error.Code.EndsWith("AlreadyExists", StringComparison.OrdinalIgnoreCase))
            return Conflict(error);

        return BadRequest(error);
    }
}
