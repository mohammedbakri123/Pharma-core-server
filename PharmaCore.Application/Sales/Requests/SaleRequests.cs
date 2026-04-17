using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Sales.Requests;

public sealed record CreateSaleCommand(int? UserId, int? CustomerId, string? Note, decimal? Discount);

public sealed record ListSalesQuery(
    int Page,
    int Limit,
    int? CustomerId,
    int? UserId,
    SaleStatus? Status,
    DateTime? From,
    DateTime? To);

public sealed record GetSaleByIdQuery(int SaleId);
public sealed record AddSaleItemCommand(int SaleId, int MedicineId, int Quantity, decimal? UnitPrice);
public sealed record UpdateSaleItemCommand(int SaleId, int ItemId, int Quantity);
public sealed record DeleteSaleItemCommand(int SaleId, int ItemId);
public sealed record CompleteSaleCommand(int SaleId, int? UserId, IReadOnlyList<SalePaymentInputDto> Payments);
