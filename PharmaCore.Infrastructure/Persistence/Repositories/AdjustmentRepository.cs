using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Entities;
using PharmaCore.Infrastructure.Utilities;
using AdjustmentModel = PharmaCore.Infrastructure.Models.Adjustment;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class AdjustmentRepository(ApplicationDbContext dbContext)
    : IAdjustmentRepository
{
    public async Task<Adjustment?> GetByIdAsync(int adjustmentId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Adjustments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AdjustmentId == adjustmentId && a.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    public async Task<IEnumerable<Adjustment>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Adjustments
            .AsNoTracking()
            .Where(a => a.IsDeleted != true)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return models.Select(Map).ToList();
    }

    public async Task<Adjustment> AddAsync(Adjustment adjustment, CancellationToken cancellationToken = default)
    {
        var model = new AdjustmentModel
        {
            MedicineId = adjustment.MedicineId,
            BatchId = adjustment.BatchId,
            Quantity = adjustment.Quantity,
            Type = (short)adjustment.Type,
            Reason = adjustment.Reason,
            UserId = adjustment.UserId,
            CreatedAt = DateTimeHelper.NormalizeTimestamp(adjustment.CreatedAt),
            IsDeleted = false
        };

        dbContext.Adjustments.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    private static Adjustment Map(AdjustmentModel model)
    {
        return Adjustment.Rehydrate(
            model.AdjustmentId,
            model.MedicineId ?? 0,
            model.BatchId ?? 0,
            model.Quantity ?? 0,
            (Domain.Enums.StockMovementType)(model.Type ?? 0),
            model.Reason,
            model.UserId ?? 0,
            model.CreatedAt ?? DateTimeHelper.GetCurrentTimestamp(),
            model.IsDeleted ?? false,
            model.DeletedAt);
    }
}