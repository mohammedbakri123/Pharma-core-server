namespace PharmaCore.API.Contracts.Medicine;

public sealed record CreateMedicineRequest(
    string Name,
    string? ArabicName,
    string? Barcode,
    int? CategoryId,
    short? Unit);
