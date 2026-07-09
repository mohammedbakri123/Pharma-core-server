using PharmaCore.Application.Common.Pagination;
using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Abstractions.Persistence;

public interface IStockMovementRepository
{
    Task<StockMovement> AddAsync(StockMovement stockMovement, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockMovement>> AddRangeAsync(IReadOnlyList<StockMovement> stockMovements, CancellationToken cancellationToken = default);
    Task<PagedResult<StockMovement>> ListByMedicineIdAsync(int medicineId, int page, int limit, CancellationToken cancellationToken = default);
}
