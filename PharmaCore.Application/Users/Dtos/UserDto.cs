namespace PharmaCore.Application.Users.Dtos;

public sealed record UserDto(
    int UserId,
    string UserName,
    string? PhoneNumber,
    string? Address,
    short Role,
    DateTime? CreatedAt);
