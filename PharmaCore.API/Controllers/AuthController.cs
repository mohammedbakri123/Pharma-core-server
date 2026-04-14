using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.Auth;
using PharmaCore.Application.Abstractions.Auth;
using PharmaCore.Application.Auth.Dtos;
using PharmaCore.Application.Auth.Interfaces;
using PharmaCore.Application.Auth.Requests;

namespace PharmaCore.API.Controllers;

/// <summary>
/// Authentication endpoints (login, logout, current user info).
/// </summary>
[Route("auth")]
[Tags("Auth")]
public class AuthController : ApiControllerBase
{
    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="loginService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">JWT token and user info.</response>
    /// <response code="400">Missing username or password.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] ILoginService loginService,
        CancellationToken cancellationToken)
    {
        return await MapAppExceptionAsync(async () =>
        {
            var response = await loginService.ExecuteAsync(
                new LoginCommand(request.UserName, request.Password), cancellationToken);
            return Ok(response);
        });
    }

    /// <summary>
    /// Revokes the current user's JWT token (logout).
    /// </summary>
    /// <response code="200">Logged out successfully.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(
        [FromServices] ITokenRevocationService tokenRevocationService,
        CancellationToken cancellationToken)
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var expiresAt = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

        if (string.IsNullOrWhiteSpace(jti) || string.IsNullOrWhiteSpace(expiresAt) || !long.TryParse(expiresAt, out var expUnix))
        {
            return Unauthorized(new { error = "Unauthorized" });
        }

        var expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
        await tokenRevocationService.RevokeAsync(jti, expiresAtUtc, cancellationToken);

        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Returns the currently authenticated user's profile.
    /// </summary>
    /// <response code="200">User profile.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Me(
        [FromServices] IGetCurrentUserService getCurrentUserService,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Unauthorized" });
        }

        return await MapAppExceptionAsync(async () =>
        {
            var user = await getCurrentUserService.ExecuteAsync(userId, cancellationToken);
            return Ok(user);
        });
    }
}
