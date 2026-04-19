using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Abstractions.Persistence;

public interface IAdjustmentRepository
{
    Task<Adjustment?> GetByIdAsync(int adjustmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Adjustment>> ListAsync(CancellationToken cancellationToken = default);
    Task<Adjustment> AddAsync(Adjustment adjustment, CancellationToken cancellationToken = default);
}