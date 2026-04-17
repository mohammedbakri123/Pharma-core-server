using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Sales.Dtos;

public sealed record SaleDto(
    int SaleId,
    int? UserId,
    int? CustomerId,
    SaleStatus Status,
    decimal TotalAmount,
    decimal Discount,
    DateTime CreatedAt,
    string? Note);

public sealed record SaleListItemDto(
    int SaleId,
    int? UserId,
    string? UserName,
    int? CustomerId,
    string? CustomerName,
    SaleStatus Status,
    decimal TotalAmount,
    decimal Discount,
    DateTime CreatedAt,
    string? Note);

public sealed record SaleItemDto(
    int SaleItemId,
    int SaleId,
    int MedicineId,
    int BatchId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

public sealed record SaleItemDetailsDto(
    int SaleItemId,
    int MedicineId,
    string? MedicineName,
    int BatchId,
    string? BatchNumber,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

public sealed record SaleDetailsDto(
    int SaleId,
    int? UserId,
    string? UserName,
    int? CustomerId,
    string? CustomerName,
    SaleStatus Status,
    decimal TotalAmount,
    decimal Discount,
    DateTime CreatedAt,
    string? Note,
    IReadOnlyList<SaleItemDetailsDto> Items);

public sealed record SaleBalanceDto(
    int SaleId,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount);

public sealed record SalePaymentInputDto(
    decimal Amount,
    PaymentMethod Method,
    string? Description);

public sealed record CompleteSaleResultDto(
    int SaleId,
    SaleStatus Status,
    decimal TotalAmount,
    decimal Discount,
    DateTime CompletedAt,
    int StockMovementsCreated,
    int PaymentsCreated,
    SaleBalanceDto Balance);
