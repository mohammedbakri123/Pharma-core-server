using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Users.Requests;

public sealed record ListUsersQuery(int Page = 1, int Limit = 20, UserRole? Role = null, string? Search = null);
