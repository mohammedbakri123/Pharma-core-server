using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Customers.Requests;

public sealed record PayCustomerDebtResult(
    int PaymentId,
    decimal Amount,
    PaymentMethod Method,
    DateTime? CreatedAt,
    IReadOnlyList<AppliedSalePayment> AppliedToSales,
    CustomerBalanceSummary CustomerBalance);

public sealed record AppliedSalePayment(
    int SaleId,
    decimal AmountApplied,
    decimal RemainingBalance);

public sealed record CustomerBalanceSummary(
    decimal TotalDebt,
    decimal NewBalance);
