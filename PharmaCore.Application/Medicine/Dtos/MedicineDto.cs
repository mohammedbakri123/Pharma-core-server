
using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Medicine.Dtos;

public sealed record MedicineDto(
    int MedicineId,
    string Name,
    string? ArabicName,
    string? Barcode,
    int? CategoryId,
    MedicineUnit? Unit,
    bool IsActive,
    DateTime? CreatedAt);