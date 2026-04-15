using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Medicine.Requests;

public sealed record UpdateMedicineCommand(
    int MedicineId,
    string? Name,
    string? ArabicName,
    string? Barcode,
    int? CategoryId,
    MedicineUnit? Unit);