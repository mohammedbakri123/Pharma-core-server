namespace PharmaCore.API.Contracts.Customers;

public sealed record UpdateCustomerRequest(
    string? Name,
    string? PhoneNumber,
    string? Address,
    string? Note);
