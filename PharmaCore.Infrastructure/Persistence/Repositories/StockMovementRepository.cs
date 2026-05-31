using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Infrastructure.Utilities;
using StockMovementModel = PharmaCore.Infrastructure.Models.StockMovement;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class StockMovementRepository(ApplicationDbContext dbContext) : IStockMovementRepository
{
    public async Task<StockMovement> AddAsync(StockMovement stockMovement, CancellationToken cancellationToken = default)
    {
        var model = ToModel(stockMovement);
        dbContext.StockMovements.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<IReadOnlyList<StockMovement>> AddRangeAsync(IReadOnlyList<StockMovement> stockMovements, CancellationToken cancellationToken = default)
    {
        var models = stockMovements.Select(ToModel).ToList();
        dbContext.StockMovements.AddRange(models);
        await dbContext.SaveChangesAsync(cancellationToken);
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
