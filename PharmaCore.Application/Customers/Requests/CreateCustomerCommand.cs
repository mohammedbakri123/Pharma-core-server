namespace PharmaCore.Application.Customers.Requests;

public sealed record CreateCustomerCommand(
    string Name,
    string? PhoneNumber,
    string? Address,
    string? Note);
