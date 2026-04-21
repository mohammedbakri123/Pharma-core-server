using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Abstractions.Persistence;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(int supplierId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Supplier>> ListAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Supplier>> ListDeletedAsync(CancellationToken cancellationToken = default);
    Task<Supplier> AddAsync(Supplier supplier, CancellationToken cancellationToken = default);
    Task<Supplier> UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int supplierId, CancellationToken cancellationToken = default);
    Task<bool> RestoreDeletedAsync(int supplierId, CancellationToken cancellationToken = default);
    Task<bool> HardDeleteAsync(int supplierId, CancellationToken cancellationToken = default);
}