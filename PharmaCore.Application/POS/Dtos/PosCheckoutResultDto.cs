using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.POS.Dtos;

public sealed record PosCheckoutResultDto(
    int SaleId,
    IReadOnlyList<int> PaymentIds,
    SaleStatus Status,
    decimal Subtotal,
    decimal Discount,
    decimal TotalAmount,
    IReadOnlyList<PosCheckoutPaymentDto> Payments,
    decimal PaidAmount,
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

public sealed record PosCheckoutPaymentDto(PaymentMethod Method, decimal Amount);
