namespace PharmaCore.Application.Auth.Dtos;

public sealed record LoginResponseDto(string Token, AuthenticatedUserDto User);
