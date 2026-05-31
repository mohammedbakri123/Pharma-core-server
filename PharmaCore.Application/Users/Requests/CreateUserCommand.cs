namespace PharmaCore.Application.Users.Requests;

public sealed record CreateUserCommand(
    string UserName,
    string Password,
    string? PhoneNumber,
    string? Address,
    short Role);
