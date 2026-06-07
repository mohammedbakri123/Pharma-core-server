using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Auth.Dtos;

public sealed record AuthenticatedUserDto(int UserId, string UserName, UserRole Role);
