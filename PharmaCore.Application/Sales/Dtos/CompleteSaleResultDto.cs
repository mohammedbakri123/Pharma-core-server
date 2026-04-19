using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Sales.Dtos;

public sealed record CompleteSaleResultDto(
    int SaleId,
    SaleStatus Status,
    decimal TotalAmount,
    decimal Discount,
    DateTime CompletedAt,
    int StockMovementsCreated,
    int PaymentsCreated,
    SaleBalanceDto Balance);