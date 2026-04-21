using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Infrastructure.Utilities;
using StockMovementModel = PharmaCore.Infrastructure.Models.StockMovement;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class StockMovementRepository : IStockMovementRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StockMovementRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StockMovement> AddAsync(StockMovement stockMovement, CancellationToken cancellationToken = default)
    {
        var model = ToModel(stockMovement);
        _dbContext.StockMovements.Add(model);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<IReadOnlyList<StockMovement>> AddRangeAsync(IReadOnlyList<StockMovement> stockMovements, CancellationToken cancellationToken = default)
    {
        var models = stockMovements.Select(ToModel).ToList();
        _dbContext.StockMovements.AddRange(models);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return models.Select(Map).ToList();
    }

    private static StockMovementModel ToModel(StockMovement stockMovement)
    {
        return new StockMovementModel
        {
            MedicineId = stockMovement.MedicineId,
            BatchId = stockMovement.BatchId,
            Quantity = stockMovement.Quantity,
            Type = (short)stockMovement.Type,
            ReferenceType = (short)stockMovement.ReferenceType,
            ReferenceId = stockMovement.ReferenceId,
            CreatedAt = DateTimeHelper.NormalizeTimestamp(stockMovement.CreatedAt ?? DateTime.UtcNow),
            IsDeleted = false
        };
    }

    private static StockMovement Map(StockMovementModel model)
    {
        return StockMovement.Rehydrate(
            model.StockMovementId,
            model.MedicineId ?? 0,
            model.BatchId ?? 0,
            model.Quantity,
            (StockMovementType)(model.Type ?? 0),
            (StockMovementReferenceType)(model.ReferenceType ?? 0),
            model.ReferenceId ?? 0,
            model.CreatedAt,
            model.IsDeleted,
            model.DeletedAt);
    }
}
