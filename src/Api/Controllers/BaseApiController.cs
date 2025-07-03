using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
    }

    protected string GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? "System User";
    }

    protected string? GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    protected string? GetUserAgent()
    {
        return HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
    }

    protected IActionResult HandleException(Exception ex)
    {
        // Log the exception here if needed
        return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
    }

    protected IActionResult CreatedAtAction<T>(string actionName, object routeValues, T value)
    {
        return base.CreatedAtAction(actionName, routeValues, value);
    }
}
