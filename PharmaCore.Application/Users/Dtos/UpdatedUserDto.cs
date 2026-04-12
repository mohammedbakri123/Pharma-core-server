namespace PharmaCore.Application.Users.Dtos;

public sealed record UpdatedUserDto(
    int UserId,
    string UserName,
    string? PhoneNumber,
    string? Address,
    short Role,
    DateTime UpdatedAt);
