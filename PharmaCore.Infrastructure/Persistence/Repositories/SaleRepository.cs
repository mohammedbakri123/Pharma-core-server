using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using SaleEntity = PharmaCore.Domain.Entities.Sale;
using SaleItemEntity = PharmaCore.Domain.Entities.SaleItem;
using SaleModel = PharmaCore.Infrastructure.Models.Sale;
using SaleItemModel = PharmaCore.Infrastructure.Models.SaleItem;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SaleRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SaleEntity?> GetByIdAsync(int saleId, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Sales
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SaleId == saleId && s.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    public async Task<SaleEntity?> GetByIdWithItemsAsync(int saleId, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Sales
            .AsNoTracking()
            .Include(s => s.SaleItems)
            .FirstOrDefaultAsync(s => s.SaleId == saleId && s.IsDeleted != true, cancellationToken);

        return model is null ? null : MapWithItems(model);
    }

    public async Task<PagedResult<SaleEntity>> ListAsync(int page, int limit, SaleStatus? status, int? userId, int? customerId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Sales
            .AsNoTracking()
            .Where(s => s.IsDeleted != true);

        if (status.HasValue)
            query = query.Where(s => s.Status == (short)status.Value);

        if (userId.HasValue)
            query = query.Where(s => s.UserId == userId.Value);

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);

        var total = await query.CountAsync(cancellationToken);
        
        var models = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<SaleEntity>(
            models.Select(Map).ToList(),
            total,
            page,
            limit > 0 ? (int)Math.Ceiling((double)total / limit) : 1);
    }

    public async Task<SaleEntity> AddAsync(SaleEntity sale, CancellationToken cancellationToken = default)
    {
        var model = new SaleModel
        {
            UserId = sale.UserId,
            CustomerId = sale.CustomerId,
            Status = (short)sale.Status,
            TotalAmount = sale.TotalAmount,
            Discount = sale.Discount,
            CreatedAt = sale.CreatedAt,
            Note = sale.Note,
            IsDeleted = false
        };

        _dbContext.Sales.Add(model);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<SaleEntity> UpdateAsync(SaleEntity sale, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Sales
            .FirstAsync(s => s.SaleId == sale.SaleId, cancellationToken);

        model.UserId = sale.UserId;
        model.CustomerId = sale.CustomerId;
        model.Status = (short)sale.Status;
        model.TotalAmount = sale.TotalAmount;
        model.Discount = sale.Discount;
        model.Note = sale.Note;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int saleId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE sales SET is_deleted = TRUE, deleted_at = NOW() WHERE sale_id = {saleId} AND is_deleted IS NOT TRUE",
            cancellationToken);

        return affectedRows > 0;
    }


    public async Task<SaleItemEntity> AddItemAsync(SaleItemEntity item, CancellationToken cancellationToken = default)
    {
        var model = new SaleItemModel
        {
            SaleId = item.SaleId,
            MedicineId = item.MedicineId,
            BatchId = item.BatchId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = item.TotalPrice
        };

        _dbContext.SaleItems.Add(model);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapItem(model);
    }

    public async Task<SaleItemEntity?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.SaleItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.SaleItemId == itemId, cancellationToken);

        return model is null ? null : MapItem(model);
    }

    public async Task<SaleItemEntity> UpdateItemAsync(SaleItemEntity item, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.SaleItems
            .FirstAsync(i => i.SaleItemId == item.SaleItemId, cancellationToken);

        model.Quantity = item.Quantity;
        model.UnitPrice = item.UnitPrice;
        model.TotalPrice = item.TotalPrice;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapItem(model);
    }

    public async Task<bool> DeleteItemAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM sale_items WHERE sale_item_id = {itemId}",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task<List<SaleItemEntity>> GetItemsBySaleIdAsync(int saleId, CancellationToken cancellationToken = default)
    {
        var models = await _dbContext.SaleItems
            .AsNoTracking()
            .Where(i => i.SaleId == saleId)
            .ToListAsync(cancellationToken);

        return models.Select(MapItem).ToList();
    }

    public async Task<decimal> GetTotalPaidAmountAsync(int saleId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .AsNoTracking()
            .Where(p => p.ReferenceType == 1 && p.ReferenceId == saleId && p.IsDeleted != true)
            .SumAsync(p => p.Amount, cancellationToken);
    }

    private static SaleEntity Map(SaleModel model)
    {
        return SaleEntity.Rehydrate(
            model.SaleId,
            model.UserId,
            model.CustomerId,
            (SaleStatus)model.Status!,
            model.TotalAmount ?? 0,
            model.Discount ?? 0,
            model.CreatedAt ?? DateTime.UtcNow,
            model.Note,
            model.IsDeleted ?? false,
            model.DeletedAt);
    }

    private static SaleEntity MapWithItems(SaleModel model)
    {
        var items = model.SaleItems?.Select(MapItem).ToList() ?? new List<SaleItemEntity>();
        return SaleEntity.Rehydrate(
            model.SaleId,
            model.UserId,
            model.CustomerId,
            (SaleStatus)model.Status!,
            model.TotalAmount ?? 0,
            model.Discount ?? 0,
            model.CreatedAt ?? DateTime.UtcNow,
            model.Note,
            model.IsDeleted ?? false,
            model.DeletedAt,
            items);
    }

    private static SaleItemEntity MapItem(SaleItemModel model)
    {
        return SaleItemEntity.Rehydrate(
            model.SaleItemId,
            model.SaleId ?? 0,
            model.MedicineId ?? 0,
            model.BatchId ?? 0,
            model.Quantity,
            model.UnitPrice ?? 0,
            model.TotalPrice ?? 0);
    }
}