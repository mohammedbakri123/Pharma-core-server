using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.POS.Dtos;

public sealed record PosCheckoutResultDto(
    int SaleId,
    int PaymentId,
    SaleStatus Status,
    decimal Subtotal,
    decimal Discount,
    decimal TotalAmount,
    PaymentMethod PaymentMethod,
    decimal PaymentAmount,
    decimal ChangeAmount,
    IReadOnlyList<PosCheckoutItemDto> Items,
    DateTime CreatedAt,
    int? CustomerId,
    string? CustomerName,
    string? UserName);

public sealed record PosCheckoutItemDto(
    int MedicineId,
    string? MedicineName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
