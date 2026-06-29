using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Infrastructure.Utilities;
using SalesReturnEntity = PharmaCore.Domain.Entities.SalesReturn;
using SalesReturnItemEntity = PharmaCore.Domain.Entities.SalesReturnItem;
using SalesReturnModel = PharmaCore.Infrastructure.Models.SalesReturn;
using SalesReturnItemModel = PharmaCore.Infrastructure.Models.SalesReturnItem;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class SalesReturnRepository(ApplicationDbContext dbContext) : ISalesReturnRepository
{

    public async Task<SalesReturnEntity?> GetByIdAsync(int salesReturnId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.SalesReturns
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.SalesReturnId == salesReturnId && r.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    public async Task<SalesReturnEntity?> GetByIdWithItemsAsync(int salesReturnId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.SalesReturns
            .AsNoTracking()
            .Include(r => r.SalesReturnItems)
            .FirstOrDefaultAsync(r => r.SalesReturnId == salesReturnId && r.IsDeleted != true, cancellationToken);

        return model is null ? null : MapWithItems(model);
    }

    public async Task<IEnumerable<SalesReturnEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.SalesReturns
            .AsNoTracking()
            .Where(r => r.IsDeleted != true)
            .ToListAsync(cancellationToken);
        return models.Select(Map).ToList();
    }

    public async Task<PagedResult<SalesReturn>> ListPagedAsync(ListSalesReturnQuery query, CancellationToken cancellationToken = default)
    {
        var filtered = dbContext.SalesReturns
                .AsNoTracking()
                .Include(r => r.User)
                .Where(r => r.IsDeleted != true);
        
        if (query.SaleId.HasValue)
            filtered = filtered.Where(r => r.SaleId == query.SaleId.Value);
            
        if (query.CustomerId.HasValue)
            filtered = filtered.Where(r => r.CustomerId == query.CustomerId.Value);
            
        if (query.UserId.HasValue)
            filtered = filtered.Where(r => r.UserId == query.UserId.Value);
            
        if (query.From.HasValue)
            filtered = filtered.Where(r => r.CreatedAt >= query.From.Value);
            
        if (query.To.HasValue)
            filtered = filtered.Where(r => r.CreatedAt <= query.To.Value);
            
        var total = await filtered.CountAsync(cancellationToken);
        var items = await filtered
            .OrderByDescending(r => r.CreatedAt)
            .Skip((query.Page - 1) * query.Limit)
            .Take(query.Limit).ToListAsync(cancellationToken);
        return  new PagedResult<SalesReturn>(items.Select(Map).ToList(), total , query.Page, query.Limit);
    }

    public async Task<IEnumerable<SalesReturnEntity>> ListDetailsAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.SalesReturns
            .AsNoTracking()
            .Include(r => r.SalesReturnItems)
            .Where(r => r.IsDeleted != true)
            .ToListAsync(cancellationToken);

        return models.Select(MapWithItems).ToList();
    }

    public async Task<IEnumerable<SalesReturnEntity>> GetByCustomerIdAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = dbContext.SalesReturns
            .AsNoTracking()
            .Where(r => r.CustomerId == customerId && r.IsDeleted != true);

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

    public async Task<IEnumerable<SalesReturn>> GetBySaleIdWithItemsAsync(int saleId,SalesReturnStatus? status, CancellationToken cancellationToken = default)
    {
        var query =   dbContext.SalesReturns
            .AsNoTracking()
            .Include(r => r.SalesReturnItems)
            .Where(r => r.SaleId == saleId && r.IsDeleted != true);
        
        if (status.HasValue)
            query = query.Where(r => r.Status == (short)status.Value);
        
        var models = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
        return models.Select(MapWithItems).ToList();

    }

    public async Task<SalesReturnEntity?> GetDetailsAsync(int salesReturnId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.SalesReturns
            .AsNoTracking()
            .Include(r => r.SalesReturnItems)
            .FirstOrDefaultAsync(r => r.SalesReturnId == salesReturnId && r.IsDeleted != true, cancellationToken);

        return model is null ? null : MapWithItems(model);
    }

    public async Task<SalesReturnEntity> AddAsync(SalesReturnEntity salesReturn, CancellationToken cancellationToken = default)
    {
        var model = new SalesReturnModel
        {
            SaleId = salesReturn.SaleId,
            CustomerId = salesReturn.CustomerId,
            UserId = salesReturn.UserId,
            Status = (short)salesReturn.Status,
            TotalAmount = salesReturn.TotalAmount,
            Note = salesReturn.Note,
            CreatedAt = DateTimeHelper.NormalizeTimestamp(salesReturn.CreatedAt),
            IsDeleted = false
        };

        dbContext.SalesReturns.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<SalesReturnEntity> UpdateAsync(SalesReturnEntity salesReturn, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.SalesReturns
            .FirstAsync(r => r.SalesReturnId == salesReturn.SalesReturnId, cancellationToken);

        model.SaleId = salesReturn.SaleId;
        model.CustomerId = salesReturn.CustomerId;
        model.UserId = salesReturn.UserId;
        model.Status = (short)salesReturn.Status;
        model.TotalAmount = salesReturn.TotalAmount;
        model.Note = salesReturn.Note;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int salesReturnId, CancellationToken cancellationToken = default)
    {
        var deletedAt = DateTimeHelper.GetCurrentTimestamp();
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE sales_returns SET is_deleted = TRUE, deleted_at = {deletedAt} WHERE sales_return_id = {salesReturnId} AND is_deleted IS NOT TRUE",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task<SalesReturnItemEntity> AddItemAsync(SalesReturnItemEntity item, CancellationToken cancellationToken = default)
    {
        var model = new SalesReturnItemModel
        {
            SalesReturnId = item.SalesReturnId,
            SaleItemId = item.SaleItemId,
            BatchId = item.BatchId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = item.TotalPrice,
            IsDeleted = false
        };

        dbContext.SalesReturnItems.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapItem(model);
    }

    public async Task<SalesReturnItemEntity?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.SalesReturnItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.SalesReturnItemId == itemId, cancellationToken);

        return model is null ? null : MapItem(model);
    }

    public async Task<List<SalesReturnItemEntity>> GetItemsBySalesReturnIdAsync(int salesReturnId, CancellationToken cancellationToken = default)
    {
        var models = await dbContext.SalesReturnItems
            .AsNoTracking()
            .Where(i => i.SalesReturnId == salesReturnId && i.IsDeleted != true)
            .ToListAsync(cancellationToken);

        return models.Select(MapItem).ToList();
    }

    public async Task<SalesReturnItemEntity> UpdateItemAsync(SalesReturnItemEntity item, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.SalesReturnItems
            .FirstAsync(i => i.SalesReturnItemId == item.SalesReturnItemId, cancellationToken);

        model.Quantity = item.Quantity;
        model.UnitPrice = item.UnitPrice;
        model.TotalPrice = item.TotalPrice;

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapItem(model);
    }

    public async Task<bool> DeleteItemAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM sales_return_items WHERE sales_return_item_id = {itemId}",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task UpdateTotalAmountAsync(int salesReturnId, CancellationToken cancellationToken = default)
    {
        var salesReturn = await dbContext.SalesReturns.FirstAsync(r => r.SalesReturnId == salesReturnId, cancellationToken);

        var itemsTotal = await dbContext.SalesReturnItems
            .AsNoTracking()
            .Where(i => i.SalesReturnId == salesReturnId && i.IsDeleted != true)
            .SumAsync(i => i.TotalPrice, cancellationToken) ?? 0m;

        salesReturn.TotalAmount = itemsTotal;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalAmountByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.SalesReturns.AsNoTracking()
            .Where(r => r.CustomerId == customerId && r.IsDeleted != true)
            .SumAsync(r => (decimal?)r.TotalAmount, cancellationToken) ?? 0m;
    }

    public async Task<bool> ExistsDraftForSaleAsync(int saleId, CancellationToken cancellationToken = default)
    {
        return await dbContext.SalesReturns
            .AnyAsync(r => r.SaleId == saleId
                && r.Status == (short)SalesReturnStatus.Draft
                && r.IsDeleted != true, cancellationToken);
    }

    public async Task<int> GetCompletedReturnQuantityBySaleItemAsync(int saleItemId, CancellationToken cancellationToken = default)
    {
        return await dbContext.SalesReturnItems
            .Where(i => i.SaleItemId == saleItemId && i.IsDeleted != true)
            .Where(i => i.SalesReturn != null
                && i.SalesReturn.Status == (short)SalesReturnStatus.Completed
                && i.SalesReturn.IsDeleted != true)
            .SumAsync(i => (int?)i.Quantity, cancellationToken) ?? 0;
    }

    private static SalesReturnEntity Map(SalesReturnModel model)
    {
        return SalesReturnEntity.Rehydrate(
            model.SalesReturnId,
            model.SaleId,
            model.CustomerId,
            model.UserId,
            model.User?.UserName,
            (SalesReturnStatus)(model.Status ?? 1),
            model.TotalAmount ?? 0m,
            model.Note,
            model.CreatedAt ?? DateTimeHelper.GetCurrentTimestamp(),
            model.IsDeleted ?? false,
            model.DeletedAt);
    }

    private static SalesReturnEntity MapWithItems(SalesReturnModel model)
    {
        var items = model.SalesReturnItems?.Select(MapItem).ToList() ?? new List<SalesReturnItemEntity>();
        return SalesReturnEntity.Rehydrate(
            model.SalesReturnId,
            model.SaleId,
            model.CustomerId,
            model.UserId,
            model.User?.UserName,
            (SalesReturnStatus)(model.Status ?? 1),
            model.TotalAmount ?? 0m,
            model.Note,
            model.CreatedAt ?? DateTimeHelper.GetCurrentTimestamp(),
            model.IsDeleted ?? false,
            model.DeletedAt,
            items);
    }

    private static SalesReturnItemEntity MapItem(SalesReturnItemModel model)
    {
        return SalesReturnItemEntity.Rehydrate(
            model.SalesReturnItemId,
            model.SalesReturnId ?? 0,
            model.SaleItemId ?? 0,
            model.BatchId ?? 0,
            model.Quantity ?? 0,
            model.UnitPrice ?? 0m,
            model.TotalPrice ?? 0m);
    }
}