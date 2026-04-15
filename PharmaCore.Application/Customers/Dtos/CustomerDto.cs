namespace PharmaCore.Application.Customers.Dtos;

public sealed record CustomerDto(
    int CustomerId,
    string Name,
    string? PhoneNumber,
    string? Address,
    string? Note,
    DateTime? CreatedAt);
