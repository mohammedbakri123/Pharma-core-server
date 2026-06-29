using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Customers.Requests;

public sealed record PayCustomerDebtCommand(
    int CustomerId,
    int? UserId,
    decimal Amount,
    PaymentMethod Method,
    string? Description);
