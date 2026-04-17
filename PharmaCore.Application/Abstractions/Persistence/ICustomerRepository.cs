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
    Task<bool> HardDeleteAsync(int customerId, CancellationToken cancellationToken = default);

    // Sales for a customer
    Task<PagedResult<CustomerSaleDto>> GetSalesAsync(int customerId, int page, int limit, short? status, CancellationToken cancellationToken = default);

    Task<CustomerDebtDto?> GetDebtAsync(int customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UnpaidSaleDto>> GetUnpaidSalesAsync(int customerId, CancellationToken cancellationToken = default);
    Task<CustomerStatementDto> GetStatementAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);

}
