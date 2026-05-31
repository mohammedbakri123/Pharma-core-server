namespace PharmaCore.API.Contracts.Users;

/// <summary>
/// Request body for creating a new user.
/// </summary>
/// <param name="UserName">Username (must be unique).</param>
/// <param name="Password">Password (minimum 6 characters).</param>
/// <param name="PhoneNumber">Optional phone number.</param>
/// <param name="Address">Optional address.</param>
/// <param name="Role">User role (see UserRole enum).</param>
public sealed record CreateUserRequest(
    string UserName,
    string Password,
    string? PhoneNumber,
    string? Address,
    short Role);
