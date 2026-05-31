using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Infrastructure.Utilities;
using PurchaseEntity = PharmaCore.Domain.Entities.Purchase;
using PurchaseItemEntity = PharmaCore.Domain.Entities.PurchaseItem;
using PurchaseModel = PharmaCore.Infrastructure.Models.Purchase;
using PurchaseItemModel = PharmaCore.Infrastructure.Models.PurchaseItem;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class PurchaseRepository(ApplicationDbContext dbContext) : IPurchaseRepository
{
    public async Task<PurchaseEntity?> GetByIdAsync(int purchaseId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Purchases
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId && p.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    public async Task<PurchaseEntity?> GetByIdWithItemsAsync(int purchaseId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Purchases
            .AsNoTracking()
            .Include(p => p.PurchaseItems)
            .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId && p.IsDeleted != true, cancellationToken);

        return model is null ? null : MapWithItems(model);
    }

    public async Task<IEnumerable<PurchaseEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Purchases
            .AsNoTracking()
            .Where(p => p.IsDeleted != true)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return models.Select(Map).ToList();
    }

    public async Task<PurchaseEntity> AddAsync(PurchaseEntity purchase, CancellationToken cancellationToken = default)
    {
        var model = new PurchaseModel
        {
            SupplierId = purchase.SupplierId,
            InvoiceNumber = purchase.InvoiceNumber,
            TotalAmount = purchase.TotalAmount,
            Status = (short)purchase.Status,
            CreatedAt = DateTimeHelper.NormalizeTimestamp(purchase.CreatedAt),
            Note = purchase.Note,
            IsDeleted = false
        };

        dbContext.Purchases.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<PurchaseEntity> UpdateAsync(PurchaseEntity purchase, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Purchases
            .FirstAsync(p => p.PurchaseId == purchase.PurchaseId, cancellationToken);

        model.SupplierId = purchase.SupplierId;
        model.InvoiceNumber = purchase.InvoiceNumber;
        model.TotalAmount = purchase.TotalAmount;
        model.Status = (short)purchase.Status;
        model.Note = purchase.Note;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int purchaseId, CancellationToken cancellationToken = default)
    {
        var deletedAt = DateTimeHelper.GetCurrentTimestamp();
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE purchases SET is_deleted = TRUE, deleted_at = NOW() WHERE purchase_id = {purchaseId} AND is_deleted IS NOT TRUE",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task<PurchaseItemEntity> AddItemAsync(PurchaseItemEntity item, CancellationToken cancellationToken = default)
    {
        var model = new PurchaseItemModel
        {
            PurchaseId = item.PurchaseId,
            MedicineId = item.MedicineId,
            BatchId = item.BatchId,
            Quantity = item.Quantity,
            PurchasePrice = item.PurchasePrice,
            SellPrice = item.SellPrice,
            ExpireDate = item.ExpireDate
        };

        dbContext.PurchaseItems.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapItem(model);
    }

    public async Task<PurchaseItemEntity?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.PurchaseItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.PurchaseItemId == itemId, cancellationToken);

        return model is null ? null : MapItem(model);
    }

    public async Task<PurchaseItemEntity> UpdateItemAsync(PurchaseItemEntity item, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.PurchaseItems
            .FirstAsync(i => i.PurchaseItemId == item.PurchaseItemId, cancellationToken);

        model.MedicineId = item.MedicineId;
        model.BatchId = item.BatchId;
        model.Quantity = item.Quantity;
        model.PurchasePrice = item.PurchasePrice;
        model.SellPrice = item.SellPrice;
        model.ExpireDate = item.ExpireDate;

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapItem(model);
    }

    public async Task<bool> DeleteItemAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM purchase_items WHERE purchase_item_id = {itemId}",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task<List<PurchaseItemEntity>> GetItemsByPurchaseIdAsync(int purchaseId, CancellationToken cancellationToken = default)
    {
        var models = await dbContext.PurchaseItems
            .AsNoTracking()
            .Where(i => i.PurchaseId == purchaseId)
            .ToListAsync(cancellationToken);

        return models.Select(MapItem).ToList();
    }

    public async Task UpdateTotalAmountAsync(int purchaseId, CancellationToken cancellationToken = default)
    {
        var totalAmount = await dbContext.PurchaseItems
            .Where(i => i.PurchaseId == purchaseId)
            .SumAsync(i => (decimal?)(i.Quantity * i.PurchasePrice) ?? 0m, cancellationToken);

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE purchases SET total_amount = {totalAmount} WHERE purchase_id = {purchaseId}",
            cancellationToken);
    }

    public async Task<IEnumerable<PurchaseEntity>> GetBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Purchases
            .AsNoTracking()
            .Where(p => p.SupplierId == supplierId && p.IsDeleted != true)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return models.Select(Map).ToList();
    }

    public async Task<decimal> GetTotalPurchasesAmountBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Purchases
            .AsNoTracking()
            .Where(p => p.SupplierId == supplierId && p.IsDeleted != true)
            .SumAsync(p => (decimal?)p.TotalAmount ?? 0m, cancellationToken);
    }

    private static PurchaseEntity Map(PurchaseModel model)
    {
        return PurchaseEntity.Rehydrate(
            model.PurchaseId,
            model.SupplierId,
            model.InvoiceNumber,
            model.TotalAmount ?? 0m,
            (PurchaseStatus)(model.Status ?? (short)PurchaseStatus.DRAFT),
            model.CreatedAt ?? DateTimeHelper.GetCurrentTimestamp(),
            model.Note,
            model.IsDeleted ?? false,
            model.DeletedAt);
    }

    private static PurchaseEntity MapWithItems(PurchaseModel model)
    {
        var items = model.PurchaseItems?.Select(MapItem).ToList() ?? new List<PurchaseItemEntity>();
        return PurchaseEntity.Rehydrate(
            model.PurchaseId,
            model.SupplierId,
            model.InvoiceNumber,
            model.TotalAmount ?? 0m,
            (PurchaseStatus)(model.Status ?? (short)PurchaseStatus.DRAFT),
            model.CreatedAt ?? DateTimeHelper.GetCurrentTimestamp(),
            model.Note,
            model.IsDeleted ?? false,
            model.DeletedAt,
            items);
    }

    private static PurchaseItemEntity MapItem(PurchaseItemModel model)
    {
        return PurchaseItemEntity.Rehydrate(
            model.PurchaseItemId,
            model.PurchaseId ?? 0,
            model.MedicineId ?? 0,
            model.BatchId ?? 0,
            model.Quantity,
            model.PurchasePrice ?? 0m,
            model.SellPrice ?? 0m,
            model.ExpireDate);
    }
}