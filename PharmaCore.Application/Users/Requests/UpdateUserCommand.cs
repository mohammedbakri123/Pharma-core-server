namespace PharmaCore.Application.Users.Requests;

public sealed record UpdateUserCommand(
    int UserId,
    string? UserName,
    string? Password,
    string? PhoneNumber,
    string? Address,
    short? Role);
