using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Users.Dtos;

public sealed record UpdatedUserDto(
    int UserId,
    string UserName,
    string? PhoneNumber,
    string? Address,
    UserRole Role,
    DateTime UpdatedAt);
