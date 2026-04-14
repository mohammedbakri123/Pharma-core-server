namespace PharmaCore.Application.Users.Dtos;

/// <summary>
/// Represents a user returned to the client.
/// </summary>
/// <param name="UserId">Unique identifier of the user.</param>
/// <param name="UserName">Username of the user.</param>
/// <param name="PhoneNumber">Phone number (may be null).</param>
/// <param name="Address">Address (may be null).</param>
/// <param name="Role">User role (see UserRole enum).</param>
/// <param name="CreatedAt">When the user was created.</param>
public sealed record UserDto(
    int UserId,
    string UserName,
    string? PhoneNumber,
    string? Address,
    short Role,
    DateTime? CreatedAt);
