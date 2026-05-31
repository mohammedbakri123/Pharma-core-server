namespace PharmaCore.Application.Suppliers.Requests;

public sealed record CreateSupplierCommand(
    string Name,
    string? PhoneNumber,
    string? Address
);
