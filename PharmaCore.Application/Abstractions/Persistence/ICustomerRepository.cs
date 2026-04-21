using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Abstractions.Persistence;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> ListAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> ListDeletedAsync(CancellationToken cancellationToken = default);
    Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<Customer> UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int customerId, CancellationToken cancellationToken = default);
    Task<bool> RestoreDeletedAsync(int customerId, CancellationToken cancellationToken = default);
    Task<bool> HardDeleteAsync(int customerId, CancellationToken cancellationToken = default);



}
