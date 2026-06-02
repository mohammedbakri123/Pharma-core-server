using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Auth.Dtos;

public sealed record CurrentUserDto(
    int UserId,
    string UserName,
    string? PhoneNumber,
    string? Address,
    UserRole Role,
    DateTime? CreatedAt);
