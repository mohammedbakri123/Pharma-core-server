namespace PharmaCore.API.Contracts.Suppliers;

public sealed record CreateSupplierRequest(
    string Name,
    string? PhoneNumber,
    string? Address
);
