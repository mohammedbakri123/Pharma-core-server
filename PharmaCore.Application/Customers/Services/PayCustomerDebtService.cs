using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class PayCustomerDebtService : IPayCustomerDebtService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PayCustomerDebtService> _logger;

    public PayCustomerDebtService(
        ICustomerRepository customerRepository,
        IPaymentRepository paymentRepository,
        ILogger<PayCustomerDebtService> logger)
    {
        _customerRepository = customerRepository;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<PayCustomerDebtResult>> ExecuteAsync(PayCustomerDebtCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(command.CustomerId, cancellationToken);
            if (customer == null)
                return ServiceResult<PayCustomerDebtResult>.Fail(ServiceErrorType.NotFound, "Customer not found.");

            if (command.Amount <= 0)
                return ServiceResult<PayCustomerDebtResult>.Fail(ServiceErrorType.Validation, "Payment amount must be greater than zero.");

            var unpaidSales = await _customerRepository.GetUnpaidSalesAsync(command.CustomerId, cancellationToken);

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

            var createdPayment = await _paymentRepository.AddAsync(payment, cancellationToken);

            var debt = await _customerRepository.GetDebtAsync(command.CustomerId, cancellationToken);
            var newBalance = debt?.TotalDebt ?? 0;

            var result = new PayCustomerDebtResult(
                createdPayment.PaymentId,
                command.Amount,
                (short)command.Method,
                DateTime.UtcNow,
                appliedToSales,
                new CustomerBalanceSummary(newBalance, newBalance - command.Amount + remaining));

            _logger.LogInformation("Payment of {Amount} applied to customer '{Name}' (ID {Id}), covering {Count} sale(s)",
                command.Amount, customer.Name, customer.CustomerId, appliedToSales.Count);

            return ServiceResult<PayCustomerDebtResult>.Ok(result);
        }
        catch (ArgumentException e)
        {
            return ServiceResult<PayCustomerDebtResult>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error paying customer debt for customer {CustomerId}", command.CustomerId);
            return ServiceResult<PayCustomerDebtResult>.Fail(ServiceErrorType.ServerError, $"Error paying customer debt: {e.Message}");
        }
    }
}
