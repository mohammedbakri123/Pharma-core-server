using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Infrastructure.Utilities;
using SaleEntity = PharmaCore.Domain.Entities.Sale;
using SaleItemEntity = PharmaCore.Domain.Entities.SaleItem;
using SaleModel = PharmaCore.Infrastructure.Models.Sale;
using SaleItemModel = PharmaCore.Infrastructure.Models.SaleItem;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class SaleRepository(ApplicationDbContext dbContext)
    : ISaleRepository
{
    public async Task<SaleEntity?> GetByIdAsync(int saleId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Sales
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SaleId == saleId && s.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    public async Task<SaleEntity?> GetByIdWithItemsAsync(int saleId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Sales
            .AsNoTracking()
            .Include(s => s.SaleItems)
            .FirstOrDefaultAsync(s => s.SaleId == saleId && s.IsDeleted != true, cancellationToken);

        return model is null ? null : MapWithItems(model);
    }

    public async Task<IEnumerable<SaleEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Sales
            .AsNoTracking()
            .Where(s => s.IsDeleted != true)
            .ToListAsync(cancellationToken);
        return models.Select(Map).ToList();
    }

    
public async Task<IEnumerable<SaleEntity>> ListDetailsAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Sales
            .AsNoTracking()
            .Include(s => s.SaleItems)
            .Where(s => s.IsDeleted != true)
            .ToListAsync(cancellationToken);

        return models.Select(MapWithItems).ToList();
    }

    public async Task<SaleEntity?> GetDetailsAsync(int saleId, CancellationToken cancellationToken = default)
    {
        return await GetByIdWithItemsAsync(saleId, cancellationToken);
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
            CreatedAt = DateTimeHelper.NormalizeTimestamp(sale.CreatedAt),
            Note = sale.Note,
            IsDeleted = false
        };

        dbContext.Sales.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<SaleEntity> UpdateAsync(SaleEntity sale, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Sales
            .FirstAsync(s => s.SaleId == sale.SaleId, cancellationToken);

        model.UserId = sale.UserId;
        model.CustomerId = sale.CustomerId;
        model.Status = (short)sale.Status;
        model.TotalAmount = sale.TotalAmount;
        model.Discount = sale.Discount;
        model.Note = sale.Note;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int saleId, CancellationToken cancellationToken = default)
    {
        // var deletedAt = DateTimeHelper.GetCurrentTimestamp();
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
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

        dbContext.SaleItems.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapItem(model);
    }

    public async Task<SaleItemEntity?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.SaleItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.SaleItemId == itemId, cancellationToken);

        return model is null ? null : MapItem(model);
    }

    public async Task<SaleItemEntity> UpdateItemAsync(SaleItemEntity item, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.SaleItems
            .FirstAsync(i => i.SaleItemId == item.SaleItemId, cancellationToken);

        model.Quantity = item.Quantity;
        model.UnitPrice = item.UnitPrice;
        model.TotalPrice = item.TotalPrice;

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapItem(model);
    }

    public async Task<bool> DeleteItemAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM sale_items WHERE sale_item_id = {itemId}",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task<List<SaleItemEntity>> GetItemsBySaleIdAsync(int saleId, CancellationToken cancellationToken = default)
    {
        var models = await dbContext.SaleItems
            .AsNoTracking()
            .Where(i => i.SaleId == saleId)
            .ToListAsync(cancellationToken);

        return models.Select(MapItem).ToList();
    }

    public async Task UpdateTotalAmountAsync(int saleId, CancellationToken cancellationToken = default)
    {
        var sale = await dbContext.Sales.FirstAsync(s => s.SaleId == saleId, cancellationToken);

        var itemsTotal = await dbContext.SaleItems
            .AsNoTracking()
            .Where(i => i.SaleId == saleId)
            .SumAsync(i => (decimal?)i.TotalPrice, cancellationToken) ?? 0m;

        sale.TotalAmount = itemsTotal;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UnpaidSaleDto>> GetUnpaidSalesByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var sales = await dbContext.Sales.AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.IsDeleted != true && s.TotalAmount > 0)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        var saleIds = sales.Select(s => s.SaleId).ToList();
        
        var paidBySaleId = await dbContext.Payments.AsNoTracking()
            .Where(p => p.ReferenceType == 1 && saleIds.Contains(p.ReferenceId) && p.IsDeleted != true)
            .GroupBy(p => p.ReferenceId)
            .Select(group => new { SaleId = group.Key, Total = group.Sum(p => p.Amount) })
            .ToDictionaryAsync(item => item.SaleId, item => item.Total, cancellationToken);

        return sales
            .Select(sale =>
            {
                var paid = paidBySaleId.TryGetValue(sale.SaleId, out var totalPaid) ? totalPaid : 0m;
                var totalAmount = sale.TotalAmount ?? 0m;
                return new UnpaidSaleDto(sale.SaleId, totalAmount, paid, totalAmount - paid, sale.CreatedAt ?? DateTimeHelper.GetCurrentTimestamp());
            })
            .Where(sale => sale.RemainingAmount > 0)
            .ToList();
    }

    public async Task<decimal> GetTotalSalesAmountByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Sales.AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.IsDeleted != true)
            .SumAsync(s => (decimal?)s.TotalAmount, cancellationToken) ?? 0m;
    }

    public async Task<IReadOnlyList<SaleEntity>> GetByCustomerIdAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Sales.AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.IsDeleted != true);

        if (from.HasValue) 
        {
            var normalizedFrom = DateTimeHelper.NormalizeTimestamp(from.Value);
            query = query.Where(s => s.CreatedAt >= normalizedFrom);
        }
        
        if (to.HasValue)
        {
            var normalizedTo = DateTimeHelper.NormalizeTimestamp(to.Value);
            query = query.Where(s => s.CreatedAt <= normalizedTo);
        }

        var models = await query.OrderBy(s => s.CreatedAt).ToListAsync(cancellationToken);
        return models.Select(Map).ToList();
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
            model.CreatedAt ?? DateTimeHelper.GetCurrentTimestamp(),
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
            model.CreatedAt ?? DateTimeHelper.GetCurrentTimestamp(),
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
            model.TotalPrice ?? 0,
            model.PurchasePrice ?? 0);
    }
}