namespace PharmaCore.API.Contracts.Users;

/// <summary>
/// Request body for updating an existing user.
/// </summary>
/// <param name="UserName">Updated username (optional).</param>
/// <param name="Password">Updated password (optional, min 6 characters).</param>
/// <param name="PhoneNumber">Updated phone number (optional).</param>
/// <param name="Address">Updated address (optional).</param>
/// <param name="Role">Updated role (optional).</param>
public sealed record UpdateUserRequest(
    string? UserName,
    string? Password,
    string? PhoneNumber,
    string? Address,
    short? Role);
