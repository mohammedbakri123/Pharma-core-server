using Microsoft.AspNetCore.Mvc;
using PharmaCore.Domain.Shared;

namespace PharmaCore.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult OkResult<T>(T data) => Ok(data);
    protected IActionResult CreatedResult<T>(T data, string? routeName = null, object? routeValues = null)
    {
        if (routeName != null)
            return CreatedAtRoute(routeName, routeValues, data);
        return Created(string.Empty, data);
    }
    protected IActionResult NoContentResult() => NoContent();

    protected IActionResult MapServiceResult<T>(ServiceResult<T> result)
    {
        return result.Error.Type switch
        {
            ServiceErrorType.None => Ok(result.Data),
            ServiceErrorType.Validation => BadRequest(ErrorResponse(result.Error.Message)),
            ServiceErrorType.Duplicate => Conflict(ErrorResponse(result.Error.Message)),
            ServiceErrorType.NotFound => NotFound(ErrorResponse(result.Error.Message)),
            ServiceErrorType.Unauthorized => Unauthorized(ErrorResponse(result.Error.Message)),
            ServiceErrorType.ServerError => StatusCode(500, ErrorResponse(result.Error.Message)),
            _ => StatusCode(500, ErrorResponse("Unknown error"))
        };
    }

    protected static object ErrorResponse(string message) => new { error = message };
}
