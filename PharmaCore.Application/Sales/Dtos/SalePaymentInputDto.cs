using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Sales.Dtos;

public sealed record SalePaymentInputDto(
    decimal Amount,
    PaymentMethod Method,
    string? Description);