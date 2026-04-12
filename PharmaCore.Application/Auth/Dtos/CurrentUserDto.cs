namespace PharmaCore.Application.Auth.Dtos;

public sealed record CurrentUserDto(
    int UserId,
    string UserName,
    string? PhoneNumber,
    string? Address,
    short Role,
    DateTime? CreatedAt);
