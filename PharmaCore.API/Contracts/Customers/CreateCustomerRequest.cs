namespace PharmaCore.API.Contracts.Customers;

public sealed record CreateCustomerRequest(
    string Name,
    string? PhoneNumber,
    string? Address,
    string? Note);
