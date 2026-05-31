namespace PharmaCore.Application.Suppliers.Requests;

public sealed record UpdateSupplierCommand(
    int SupplierId,
    string? Name,
    string? PhoneNumber,
    string? Address
);
