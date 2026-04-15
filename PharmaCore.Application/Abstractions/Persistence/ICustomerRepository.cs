using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Abstractions.Persistence;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<PagedResult<Customer>> ListAsync(int page, int limit, string? searchTerm, CancellationToken cancellationToken = default);
    Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<Customer> UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int customerId, CancellationToken cancellationToken = default);

    // Sales for a customer
    Task<PagedResult<CustomerSaleDto>> GetSalesAsync(int customerId, int page, int limit, short? status, CancellationToken cancellationToken = default);

    // Debt calculation
    Task<CustomerDebtDto?> GetDebtAsync(int customerId, CancellationToken cancellationToken = default);

    // Unpaid sales
    Task<IReadOnlyList<UnpaidSaleDto>> GetUnpaidSalesAsync(int customerId, CancellationToken cancellationToken = default);

    // Statement entries
    Task<CustomerStatementDto> GetStatementAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);

    // Payment creation (used by pay-debt service)
    Task<int> CreatePaymentAsync(int referenceType, int referenceId, short? method, decimal amount, string? description, int? userId, CancellationToken cancellationToken = default);

    // Get payments for a sale
    Task<decimal> GetTotalPaidForSaleAsync(int saleId, CancellationToken cancellationToken = default);
}
