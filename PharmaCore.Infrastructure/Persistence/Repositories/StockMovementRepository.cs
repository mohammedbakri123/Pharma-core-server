using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
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

    public async Task<PagedResult<StockMovement>> ListByMedicineIdAsync(int medicineId, int page, int limit, CancellationToken cancellationToken = default)
    {
        var query = dbContext.StockMovements
            .AsNoTracking()
            .Include(sm => sm.Batch)
            .Include(sm => sm.Medicine)
            .Where(sm => sm.MedicineId == medicineId && sm.IsDeleted != true);

        var total = await query.CountAsync(cancellationToken);

        var models = await query
            .OrderByDescending(sm => sm.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<StockMovement>(
            models.Select(Map).ToList(),
            total,
            page,
            limit);
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
        var movement = StockMovement.Rehydrate(
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

        movement.MedicineName = model.Medicine?.Name;
        movement.BatchNumber = model.Batch?.BatchNumber;

        return movement;
    }
}
