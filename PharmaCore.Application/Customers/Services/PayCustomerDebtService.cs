using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class PayCustomerDebtService(
    ICustomerRepository customerRepository,
    IPaymentRepository paymentRepository,
    ILogger<PayCustomerDebtService> logger)
    : IPayCustomerDebtService
{
    public async Task<ServiceResult<PayCustomerDebtResult>> ExecuteAsync(PayCustomerDebtCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await customerRepository.GetByIdAsync(command.CustomerId, cancellationToken);
            if (customer == null)
                return ServiceResult<PayCustomerDebtResult>.Fail(ServiceErrorType.NotFound, "Customer not found.");

            if (command.Amount <= 0)
                return ServiceResult<PayCustomerDebtResult>.Fail(ServiceErrorType.Validation, "Payment amount must be greater than zero.");

            var unpaidSales = await customerRepository.GetUnpaidSalesAsync(command.CustomerId, cancellationToken);

            var appliedToSales = new List<AppliedSalePayment>();
            var remaining = command.Amount;

            foreach (var sale in unpaidSales.OrderBy(s => s.CreatedAt))
            {
                if (remaining <= 0)
                    break;

                var amountToApply = Math.Min(remaining, sale.RemainingAmount);
                appliedToSales.Add(new AppliedSalePayment(sale.SaleId, amountToApply, sale.RemainingAmount - amountToApply));
                remaining -= amountToApply;
            }

            var referenceSaleId = appliedToSales.FirstOrDefault()?.SaleId;
            if (!referenceSaleId.HasValue)
                return ServiceResult<PayCustomerDebtResult>.Fail(ServiceErrorType.Validation, "Customer has no unpaid sales to apply this payment to.");

            var payment = Payment.Create(
                PaymentType.INCOMING,
                PaymentReferenceType.SALE,
                referenceSaleId.Value,
                command.Method,
                null,
                command.Amount,
                command.Description);

            var createdPayment = await paymentRepository.AddAsync(payment, cancellationToken);

            var debt = await customerRepository.GetDebtAsync(command.CustomerId, cancellationToken);
            var newBalance = debt?.TotalDebt ?? 0;

            var result = new PayCustomerDebtResult(
                createdPayment.PaymentId,
                command.Amount,
                (short)command.Method,
                DateTime.UtcNow,
                appliedToSales,
                new CustomerBalanceSummary(newBalance, newBalance - command.Amount + remaining));

            logger.LogInformation("Payment of {Amount} applied to customer '{Name}' (ID {Id}), covering {Count} sale(s)",
                command.Amount, customer.Name, customer.CustomerId, appliedToSales.Count);

            return ServiceResult<PayCustomerDebtResult>.Ok(result);
        }
        catch (ArgumentException e)
        {
            return ServiceResult<PayCustomerDebtResult>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error paying customer debt for customer {CustomerId}", command.CustomerId);
            return ServiceResult<PayCustomerDebtResult>.Fail(ServiceErrorType.ServerError, $"Error paying customer debt: {e.Message}, {e.InnerException}, {e.StackTrace},  {e.Source}");
        }
    }
}
