namespace PharmaCore.Application.Users.Requests;

public sealed record ListDeletedUsersQuery(
    int Page = 1,
    int Limit = 20,
    short? Role = null,
    string? Search = null);
