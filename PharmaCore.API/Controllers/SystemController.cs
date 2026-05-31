using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.Application.System.Dtos;
using PharmaCore.Application.System.Interfaces;

namespace PharmaCore.API.Controllers;

/// <summary>
/// System operations: health, backup, restore.
/// </summary>
[Route("")]
[Tags("System")]
public class SystemController : ApiControllerBase
{
    /// <summary>
    /// Check system status.
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HealthCheckDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealth(
        [FromServices] IGetHealthCheckService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Backup database.
    /// </summary>
    [HttpPost("backup")]
    [Authorize]
    [ProducesResponseType(typeof(BackupResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Backup(
        [FromBody] BackupRequest? request,
        [FromServices] IBackupDatabaseService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(request?.BackupName, cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Restore database (dangerous - requires ADMIN role).
    /// </summary>
    [HttpPost("restore")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Restore(
        [FromBody] RestoreRequest request,
        [FromServices] IRestoreDatabaseService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(request.BackupFile, cancellationToken);
        
        if (!result.Success)
            return MapServiceResult(result);
            
        return Ok(new { message = "Database restored successfully" });
    }
}

public sealed record BackupRequest(string? BackupName);
public sealed record RestoreRequest(string BackupFile);
