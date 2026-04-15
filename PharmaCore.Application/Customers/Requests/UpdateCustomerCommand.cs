namespace PharmaCore.Application.Customers.Requests;

public sealed record UpdateCustomerCommand(
    int CustomerId,
    string? Name,
    string? PhoneNumber,
    string? Address,
    string? Note);
