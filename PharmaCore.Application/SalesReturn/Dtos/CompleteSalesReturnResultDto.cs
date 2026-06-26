using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.SalesReturn.Dtos;

public sealed record CompleteSalesReturnResultDto(
    int SalesReturnId,
    SalesReturnStatus Status,
    decimal TotalAmount,
    DateTime CompletedAt,
    int StockMovementsCreated);
