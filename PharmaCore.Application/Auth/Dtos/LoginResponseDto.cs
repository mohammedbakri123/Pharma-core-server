namespace PharmaCore.Application.Auth.Dtos;

/// <summary>
/// Response returned after a successful login.
/// </summary>
/// <param name="Token">JWT access token.</param>
/// <param name="User">Authenticated user details.</param>
public sealed record LoginResponseDto(string Token, AuthenticatedUserDto User);
