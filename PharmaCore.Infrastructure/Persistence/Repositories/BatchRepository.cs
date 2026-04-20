using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Entities;
using BatchModel = PharmaCore.Infrastructure.Models.Batch;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class BatchRepository : IBatchRepository
{
    private readonly ApplicationDbContext _dbContext;

    public BatchRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Batch?> GetByIdAsync(int batchId, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Batches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BatchId == batchId && b.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    public async Task<List<Batch>> ListAvailableByMedicineAsync(int medicineId, CancellationToken cancellationToken = default)
    {
        var models = await _dbContext.Batches
            .AsNoTracking()
            .Where(b => b.MedicineId == medicineId && b.IsDeleted != true && b.QuantityRemaining > 0)
            .OrderBy(b => b.ExpireDate)
            .ThenBy(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return models.Select(Map).ToList();
    }

    public async Task<Batch> AddAsync(Batch batch, CancellationToken cancellationToken = default)
    {
        var model = new BatchModel
        {
            MedicineId = batch.MedicineId,
            BatchNumber = batch.BatchNumber,
            QuantityRemaining = batch.QuantityRemaining,
            QuantityEntered = batch.QuantityEntered,
            PurchasePrice = batch.PurchasePrice,
            SellPrice = batch.SellPrice,
            ExpireDate = batch.ExpireDate,
            CreatedAt = batch.CreatedAt ?? DateTime.UtcNow,
            IsDeleted = false
        };

        _dbContext.Batches.Add(model);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<Batch> UpdateAsync(Batch batch, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Batches
            .FirstAsync(b => b.BatchId == batch.BatchId && b.IsDeleted != true, cancellationToken);

        model.BatchNumber = batch.BatchNumber;
        model.QuantityRemaining = batch.QuantityRemaining;
        model.QuantityEntered = batch.QuantityEntered;
        model.PurchasePrice = batch.PurchasePrice;
        model.SellPrice = batch.SellPrice;
        model.ExpireDate = batch.ExpireDate;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<int> DecrementBatchStockAsync(int batchId, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            return 0;

        var batch = await GetByIdAsync(batchId, cancellationToken);
        if (batch is null)
            return 0;

        try
        {
            batch.DecreaseStock(quantity);
            await UpdateAsync(batch, cancellationToken);
            return 1;
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
    }

    public async Task<int> IncrementBatchStockAsync(int batchId, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            return 0;

        var batch = await GetByIdAsync(batchId, cancellationToken);
        if (batch is null)
            return 0;

        try
        {
            batch.IncreaseStock(quantity);
            await UpdateAsync(batch, cancellationToken);
            return 1;
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
    }

    private static Batch Map(BatchModel model)
    {
        return Batch.Rehydrate(
            model.BatchId,
            model.MedicineId,
            model.BatchNumber,
            model.QuantityRemaining,
            model.QuantityEntered,
            model.PurchasePrice,
            model.SellPrice,
            model.ExpireDate,
            model.CreatedAt,
            model.IsDeleted,
            model.DeletedAt);
    }
}
