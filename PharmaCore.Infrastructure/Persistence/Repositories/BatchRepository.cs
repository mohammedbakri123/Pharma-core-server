using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Infrastructure.Utilities;
using BatchModel = PharmaCore.Infrastructure.Models.Batch;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class BatchRepository(ApplicationDbContext dbContext) : IBatchRepository
{
    public async Task<Batch?> GetByIdAsync(int batchId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Batches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BatchId == batchId && b.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    public async Task<List<Batch>> ListAvailableByMedicineAsync(int medicineId, CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Batches
            .AsNoTracking()
            .Where(b => b.MedicineId == medicineId && b.IsDeleted != true && b.QuantityRemaining > 0)
            .OrderBy(b => b.ExpireDate)
            .ThenBy(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return models.Select(Map).ToList();
    }

    public async Task<PagedResult<StockAlertDto>> GetStockAlertsAsync(
        int? lowStockThreshold,
        int? expiringDays,
        string? searchTerm,
        int page,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var filterByLowStock = lowStockThreshold.HasValue;
        var filterByExpiry = expiringDays.HasValue;
        var cutoffDate = filterByExpiry
            ? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(expiringDays!.Value))
            : default;

        var query = from m in dbContext.Medicines.Include(m => m.Category)
                    where m.IsDeleted != true
                    select new
                    {
                        m.MedicineId,
                        m.Name,
                        m.ArabicName,
                        m.Barcode,
                        CategoryName = m.Category != null ? m.Category.CategoryName : null,
                        Unit = (MedicineUnit?)m.Unit,
                        TotalStock = m.Batches
                            .Where(b => b.IsDeleted != true && b.QuantityRemaining > 0)
                            .Sum(b => (int?)b.QuantityRemaining) ?? 0,
                        NearestExpireDate = m.Batches
                            .Where(b => b.IsDeleted != true && b.QuantityRemaining > 0 && b.ExpireDate != null)
                            .Min(b => b.ExpireDate),
                        HasExpiringBatch = filterByExpiry && m.Batches.Any(b => b.IsDeleted != true && b.QuantityRemaining > 0 &&
                            b.ExpireDate != null && b.ExpireDate <= cutoffDate)
                    };

        if (filterByLowStock || filterByExpiry)
        {
            query = query.Where(x =>
                (filterByLowStock && x.TotalStock <= lowStockThreshold!.Value) ||
                (filterByExpiry && x.HasExpiringBatch));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.Name, $"{term}%") ||
                (x.ArabicName != null && EF.Functions.ILike(x.ArabicName, $"{term}%")) ||
                (x.Barcode != null && EF.Functions.ILike(x.Barcode, $"{term}%")));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.TotalStock)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(x =>
        {
            var isExpiring = filterByExpiry && x.HasExpiringBatch;
            var status = x.TotalStock <= 5 ? StockStatus.Critical
                : x.TotalStock <= 10 ? StockStatus.LowStock
                : StockStatus.Available;

            return new StockAlertDto(
                x.MedicineId,
                x.Name,
                x.ArabicName,
                x.Barcode,
                x.CategoryName,
                x.Unit,
                x.TotalStock,
                status,
                x.NearestExpireDate,
                isExpiring);
        }).ToList();

        return new PagedResult<StockAlertDto>(dtos, total, page, limit);
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
            CreatedAt = DateTimeHelper.NormalizeTimestamp(batch.CreatedAt ?? DateTime.UtcNow),
            IsDeleted = false
        };

        dbContext.Batches.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<Batch> UpdateAsync(Batch batch, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Batches
            .FirstAsync(b => b.BatchId == batch.BatchId && b.IsDeleted != true, cancellationToken);

        model.BatchNumber = batch.BatchNumber;
        model.QuantityRemaining = batch.QuantityRemaining;
        model.QuantityEntered = batch.QuantityEntered;
        model.PurchasePrice = batch.PurchasePrice;
        model.SellPrice = batch.SellPrice;
        model.ExpireDate = batch.ExpireDate;

        await dbContext.SaveChangesAsync(cancellationToken);
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
            model.QuantityEntered ?? 0,
            model.PurchasePrice,
            model.SellPrice,
            model.ExpireDate,
            model.CreatedAt,
            model.IsDeleted,
            model.DeletedAt);
    }
}
