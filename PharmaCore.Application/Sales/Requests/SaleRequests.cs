using PharmaCore.Application.Sales.Dtos;

namespace PharmaCore.Application.Sales.Requests;

public sealed record UpdateSaleItemCommand(int SaleId, int ItemId, int Quantity);
public sealed record DeleteSaleItemCommand(int SaleId, int ItemId);
public sealed record CompleteSaleCommand(int SaleId, int? UserId, IReadOnlyList<SalePaymentInputDto> Payments);
