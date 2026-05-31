namespace PharmaCore.API.Contracts.Suppliers;

public sealed record UpdateSupplierRequest(
    string? Name,
    string? PhoneNumber,
    string? Address
);
