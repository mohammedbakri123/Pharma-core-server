using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Abstractions.Persistence;

public interface IStockMovementRepository
{
    Task<StockMovement> AddAsync(StockMovement stockMovement, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockMovement>> AddRangeAsync(IReadOnlyList<StockMovement> stockMovements, CancellationToken cancellationToken = default);
}
