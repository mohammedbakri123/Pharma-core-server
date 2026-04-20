using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Abstractions.Persistence;

public interface IBatchRepository
{
    Task<Batch?> GetByIdAsync(int batchId, CancellationToken cancellationToken = default);
    Task<List<Batch>> ListAvailableByMedicineAsync(int medicineId, CancellationToken cancellationToken = default);
    Task<Batch> AddAsync(Batch batch, CancellationToken cancellationToken = default);
    Task<Batch> UpdateAsync(Batch batch, CancellationToken cancellationToken = default);
    Task<int> DecrementBatchStockAsync(int batchId, int quantity, CancellationToken cancellationToken = default);
}
