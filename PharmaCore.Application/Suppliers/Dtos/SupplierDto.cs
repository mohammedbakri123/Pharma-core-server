namespace PharmaCore.Application.Suppliers.Dtos;

/// <summary>
/// Represents a supplier returned to the client.
/// </summary>
/// <param name="SupplierId">Unique identifier of the supplier.</param>
/// <param name="Name">Name of the supplier.</param>
/// <param name="PhoneNumber">Phone number of the supplier.</param>
/// <param name="Address">Address of the supplier.</param>
/// <param name="CreatedAt">Creation timestamp.</param>
public sealed record SupplierDto(
    int SupplierId,
    string Name,
    string? PhoneNumber,
    string? Address,
    DateTime? CreatedAt
);
