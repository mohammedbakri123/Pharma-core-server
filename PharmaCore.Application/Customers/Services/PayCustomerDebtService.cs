using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class PayCustomerDebtService : IPayCustomerDebtService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerFinancialRepository _customerFinancialRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PayCustomerDebtService> _logger;

    public PayCustomerDebtService(
        ICustomerRepository customerRepository,
        ICustomerFinancialRepository customerFinancialRepository,
        IPaymentRepository paymentRepository,
        ILogger<PayCustomerDebtService> logger)
    {
        _customerRepository = customerRepository;
        _customerFinancialRepository = customerFinancialRepository;
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

            var unpaidSales = await _customerFinancialRepository.GetUnpaidSalesAsync(command.CustomerId, cancellationToken);

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

            var paymentId = await _paymentRepository.CreateAsync(
                PaymentType.INCOMING,
                PaymentReferenceType.SALE,
                0,
                command.Method,
                command.Amount,
                command.Description,
                null,
                cancellationToken);

            var debt = await _customerFinancialRepository.GetDebtAsync(command.CustomerId, cancellationToken);
            var newBalance = debt?.TotalDebt ?? 0;

            var result = new PayCustomerDebtResult(
                paymentId,
                command.Amount,
                (short)command.Method,
                DateTime.UtcNow,
                appliedToSales,
                new CustomerBalanceSummary(newBalance, newBalance - command.Amount + remaining));

            _logger.LogInformation("Payment of {Amount} applied to customer '{Name}' (ID {Id}), covering {Count} sale(s)",
                command.Amount, customer.Name, customer.CustomerId, appliedToSales.Count);

            return ServiceResult<PayCustomerDebtResult>.Ok(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error paying customer debt for customer {CustomerId}", command.CustomerId);
            return ServiceResult<PayCustomerDebtResult>.Fail(ServiceErrorType.ServerError, $"Error paying customer debt: {e.Message}");
        }
    }
}
