using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Entities;
using PharmaCore.Infrastructure.Utilities;
using PurchaseReturnEntity = PharmaCore.Domain.Entities.PurchaseReturn;
using PurchaseReturnItemEntity = PharmaCore.Domain.Entities.PurchaseReturnItem;
using PurchaseReturnModel = PharmaCore.Infrastructure.Models.PurchaseReturn;
using PurchaseReturnItemModel = PharmaCore.Infrastructure.Models.PurchaseReturnItem;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class PurchaseReturnRepository(ApplicationDbContext dbContext) : IPurchaseReturnRepository
{
    public async Task<PurchaseReturnEntity?> GetByIdAsync(int purchaseReturnId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.PurchaseReturns
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.PurchaseReturnId == purchaseReturnId && r.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    public async Task<PurchaseReturnEntity?> GetByIdWithItemsAsync(int purchaseReturnId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.PurchaseReturns
            .AsNoTracking()
            .Include(r => r.PurchaseReturnItems)
            .FirstOrDefaultAsync(r => r.PurchaseReturnId == purchaseReturnId && r.IsDeleted != true, cancellationToken);

        return model is null ? null : MapWithItems(model);
    }

    public async Task<IEnumerable<PurchaseReturnEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.PurchaseReturns
            .AsNoTracking()
            .Where(r => r.IsDeleted != true)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return models.Select(Map).ToList();
    }

    public async Task<IEnumerable<PurchaseReturnEntity>> GetBySupplierIdAsync(int supplierId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = dbContext.PurchaseReturns
            .AsNoTracking()
            .Where(r => r.SupplierId == supplierId && r.IsDeleted != true);

        if (from.HasValue)
        {
            var normalizedFrom = DateTimeHelper.NormalizeTimestamp(from.Value);
            query = query.Where(r => r.CreatedAt >= normalizedFrom);
        }

        if (to.HasValue)
        {
            var normalizedTo = DateTimeHelper.NormalizeTimestamp(to.Value);
            query = query.Where(r => r.CreatedAt <= normalizedTo);
        }

        var models = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return models.Select(Map).ToList();
    }

    public async Task<PurchaseReturnEntity> AddAsync(PurchaseReturnEntity purchaseReturn, CancellationToken cancellationToken = default)
    {
        var model = new PurchaseReturnModel
        {
            PurchaseId = purchaseReturn.PurchaseId,
            SupplierId = purchaseReturn.SupplierId,
            UserId = purchaseReturn.UserId,
            TotalAmount = purchaseReturn.TotalAmount,
            Note = purchaseReturn.Note,
            CreatedAt = DateTimeHelper.NormalizeTimestamp(purchaseReturn.CreatedAt),
            IsDeleted = false
        };

        dbContext.PurchaseReturns.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<PurchaseReturnEntity> UpdateAsync(PurchaseReturnEntity purchaseReturn, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.PurchaseReturns
            .FirstAsync(r => r.PurchaseReturnId == purchaseReturn.PurchaseReturnId, cancellationToken);

        model.PurchaseId = purchaseReturn.PurchaseId;
        model.SupplierId = purchaseReturn.SupplierId;
        model.TotalAmount = purchaseReturn.TotalAmount;
        model.Note = purchaseReturn.Note;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int purchaseReturnId, CancellationToken cancellationToken = default)
    {
        var deletedAt = DateTimeHelper.GetCurrentTimestamp();
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE purchase_returns SET is_deleted = TRUE, deleted_at = NOW() WHERE purchase_return_id = {purchaseReturnId} AND is_deleted IS NOT TRUE",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task<PurchaseReturnItemEntity> AddItemAsync(PurchaseReturnItemEntity item, CancellationToken cancellationToken = default)
    {
        var model = new PurchaseReturnItemModel
        {
            PurchaseReturnId = item.PurchaseReturnId,
            PurchaseItemId = item.PurchaseItemId,
            BatchId = item.BatchId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = item.TotalPrice
        };

        dbContext.PurchaseReturnItems.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapItem(model);
    }

    public async Task<PurchaseReturnItemEntity?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.PurchaseReturnItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.PurchaseReturnItemId == itemId, cancellationToken);

        return model is null ? null : MapItem(model);
    }

    public async Task<List<PurchaseReturnItemEntity>> GetItemsByPurchaseReturnIdAsync(int purchaseReturnId, CancellationToken cancellationToken = default)
    {
        var models = await dbContext.PurchaseReturnItems
            .AsNoTracking()
            .Where(i => i.PurchaseReturnId == purchaseReturnId)
            .ToListAsync(cancellationToken);

        return models.Select(MapItem).ToList();
    }

    public async Task<PurchaseReturnItemEntity> UpdateItemAsync(PurchaseReturnItemEntity item, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.PurchaseReturnItems
            .FirstAsync(i => i.PurchaseReturnItemId == item.PurchaseReturnItemId, cancellationToken);

        model.PurchaseItemId = item.PurchaseItemId;
        model.BatchId = item.BatchId;
        model.Quantity = item.Quantity;
        model.UnitPrice = item.UnitPrice;
        model.TotalPrice = item.TotalPrice;

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapItem(model);
    }
    
    public async Task<bool> DeleteItemAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM purchase_return_items WHERE purchase_return_item_id = {itemId}",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task UpdateTotalAmountAsync(int purchaseReturnId, CancellationToken cancellationToken = default)
    {
        var totalAmount = await dbContext.PurchaseReturnItems
            .Where(i => i.PurchaseReturnId == purchaseReturnId)
            .SumAsync(i => (decimal?)(i.Quantity * i.UnitPrice) ?? 0m, cancellationToken);

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE purchase_returns SET total_amount = {totalAmount} WHERE purchase_return_id = {purchaseReturnId}",
            cancellationToken);
    }

    public async Task<decimal> GetTotalAmountBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default)
    {
        return await dbContext.PurchaseReturns
            .AsNoTracking()
            .Where(r => r.SupplierId == supplierId && r.IsDeleted != true)
            .SumAsync(r => (decimal?)r.TotalAmount ?? 0m, cancellationToken);
    }

    private static PurchaseReturnEntity Map(PurchaseReturnModel model)
    {
        return PurchaseReturnEntity.Rehydrate(
            model.PurchaseReturnId,
            model.PurchaseId,
            model.SupplierId,
            model.UserId,
            model.TotalAmount ?? 0m,
            model.Note,
            model.CreatedAt ?? DateTimeHelper.GetCurrentTimestamp(),
            model.IsDeleted ?? false,
            model.DeletedAt);
    }

    private static PurchaseReturnEntity MapWithItems(PurchaseReturnModel model)
    {
        var items = model.PurchaseReturnItems?.Select(MapItem).ToList() ?? new List<PurchaseReturnItemEntity>();
        return PurchaseReturnEntity.Rehydrate(
            model.PurchaseReturnId,
            model.PurchaseId,
            model.SupplierId,
            model.UserId,
            model.TotalAmount ?? 0m,
            model.Note,
            model.CreatedAt ?? DateTimeHelper.GetCurrentTimestamp(),
            model.IsDeleted ?? false,
            model.DeletedAt,
            items);
    }

    private static PurchaseReturnItemEntity MapItem(PurchaseReturnItemModel model)
    {
        return PurchaseReturnItemEntity.Rehydrate(
            model.PurchaseReturnItemId,
            model.PurchaseReturnId ?? 0,
            model.PurchaseItemId ?? 0,
            model.BatchId ?? 0,
            model.Quantity ?? 0,
            model.UnitPrice ?? 0m,
            model.TotalPrice ?? 0m);
    }
}