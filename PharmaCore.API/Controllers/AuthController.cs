using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.Auth;
using PharmaCore.Application.Abstractions.Auth;
using PharmaCore.Application.Auth.Requests;
using PharmaCore.Application.Auth.Services;

namespace PharmaCore.API.Controllers;

[Route("auth")]
public class AuthController : ApiControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] LoginService loginService,
        CancellationToken cancellationToken)
    {
        return await MapAppExceptionAsync(async () =>
        {
            var response = await loginService.ExecuteAsync(
                new LoginCommand(request.UserName, request.Password), cancellationToken);
            return Ok(response);
        });
    }

    [HttpPost("logout")]
    [Authorize]
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

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(
        [FromServices] GetCurrentUserService getCurrentUserService,
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
