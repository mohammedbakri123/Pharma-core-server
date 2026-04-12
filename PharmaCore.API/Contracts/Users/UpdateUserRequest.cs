namespace PharmaCore.API.Contracts.Users;

public sealed record UpdateUserRequest(
    string? UserName,
    string? Password,
    string? PhoneNumber,
    string? Address,
    short? Role);
