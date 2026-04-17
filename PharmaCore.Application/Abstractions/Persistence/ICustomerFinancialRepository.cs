using PharmaCore.Application.Customers.Dtos;

namespace PharmaCore.Application.Abstractions.Persistence;

public interface ICustomerFinancialRepository
{
    Task<CustomerDebtDto?> GetDebtAsync(int customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UnpaidSaleDto>> GetUnpaidSalesAsync(int customerId, CancellationToken cancellationToken = default);
    Task<CustomerStatementDto> GetStatementAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
