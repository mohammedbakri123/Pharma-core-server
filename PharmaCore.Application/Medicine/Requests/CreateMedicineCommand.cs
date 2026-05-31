using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Medicine.Requests;

public sealed record CreateMedicineCommand(
    string Name,
    string? ArabicName,
    string? Barcode,
    int? CategoryId,
    MedicineUnit? Unit);
