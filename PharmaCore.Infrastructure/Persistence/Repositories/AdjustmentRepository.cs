using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Entities;
using PharmaCore.Infrastructure.Utilities;
using AdjustmentModel = PharmaCore.Infrastructure.Models.Adjustment;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class AdjustmentRepository : IAdjustmentRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IBatchRepository _batchRepository;
    private readonly IStockMovementRepository _stockMovementRepository;

    public AdjustmentRepository(
        ApplicationDbContext dbContext,
        IBatchRepository batchRepository,
        IStockMovementRepository stockMovementRepository)
    {
        _dbContext = dbContext;
        _batchRepository = batchRepository;
        _stockMovementRepository = stockMovementRepository;
    }

    public async Task<Adjustment?> GetByIdAsync(int adjustmentId, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Adjustments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AdjustmentId == adjustmentId && a.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    public async Task<IEnumerable<Adjustment>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await _dbContext.Adjustments
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

        _dbContext.Adjustments.Add(model);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var stockMovement = Domain.Entities.StockMovement.Create(
            adjustment.MedicineId,
            adjustment.BatchId,
            adjustment.Quantity,
            adjustment.Type,
            Domain.Enums.StockMovementReferenceType.ADJUSTMENT,
            model.AdjustmentId);

        await _stockMovementRepository.AddAsync(stockMovement, cancellationToken);

        if (adjustment.Type == Domain.Enums.StockMovementType.IN)
        {
            await _batchRepository.GetByIdAsync(adjustment.BatchId, cancellationToken);
            var batch = await _batchRepository.GetByIdAsync(adjustment.BatchId, cancellationToken);
            if (batch is not null)
            {
                batch.IncreaseStock(adjustment.Quantity);
                await _batchRepository.UpdateAsync(batch, cancellationToken);
            }
        }
        else if (adjustment.Type == Domain.Enums.StockMovementType.OUT)
        {
            var batch = await _batchRepository.GetByIdAsync(adjustment.BatchId, cancellationToken);
            if (batch is not null)
            {
                batch.DecreaseStock(adjustment.Quantity);
                await _batchRepository.UpdateAsync(batch, cancellationToken);
            }
        }

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
            model.CreatedAt ?? DateTime.UtcNow,
            model.IsDeleted ?? false,
            model.DeletedAt);
    }
}