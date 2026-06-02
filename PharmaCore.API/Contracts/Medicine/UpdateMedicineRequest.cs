using PharmaCore.Domain.Enums;

namespace PharmaCore.API.Contracts.Medicine;

public sealed record UpdateMedicineRequest(
    string? Name,
    string? ArabicName,
    string? Barcode,
    int? CategoryId,
    MedicineUnit? Unit);
