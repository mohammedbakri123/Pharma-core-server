using PharmaCore.Domain.Enums;

namespace PharmaCore.API.Contracts.Sales;

/// <summary>
/// Represents a payment for a sale.
/// </summary>
/// <param name="Amount">Payment amount.</param>
/// <param name="Method">Payment method (see PaymentMethod enum).</param>
/// <param name="Description">Optional description.</param>
public sealed record SalePaymentRequest(
    decimal Amount,
    PaymentMethod Method,
    string? Description);