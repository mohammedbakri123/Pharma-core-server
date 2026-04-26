using PharmaCore.Domain.Enums;

namespace PharmaCore.API.Contracts.Purchases;

/// <summary>
/// Request body for adding a supplier payment to a purchase.
/// </summary>
/// <param name="Method">Payment method.</param>
/// <param name="Amount">Payment amount.</param>
/// <param name="Description">Optional payment description.</param>
public sealed record CreatePurchasePaymentRequest(
    PaymentMethod Method,
    decimal Amount,
    string? Description);
