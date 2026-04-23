namespace PharmaCore.Application.POS.Dtos;

public sealed record PosMedicineDto(
    int MedicineId,
    string Name,
    string? ArabicName,
    string? Barcode,
    string? Unit,
    decimal SellPrice,
    int CurrentStock
);
