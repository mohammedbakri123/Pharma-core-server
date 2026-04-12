namespace PharmaCore.API.Contracts.Users;

public sealed record CreateUserRequest(
    string UserName,
    string Password,
    string? PhoneNumber,
    string? Address,
    short Role);
