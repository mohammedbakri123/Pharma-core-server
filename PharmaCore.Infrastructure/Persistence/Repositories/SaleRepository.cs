using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Sales.Dtos;
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
    private readonly IBatchRepository _batchRepository;

    public SaleRepository(ApplicationDbContext dbContext, IBatchRepository batchRepository)
    {
        _dbContext = dbContext;
        _batchRepository = batchRepository;
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

    public async Task<PagedResult<SaleEntity>> ListAsync(
        int page,
        int limit,
        SaleStatus? status,
        int? userId,
        int? customerId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
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

        if (from.HasValue)
            query = query.Where(s => s.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(s => s.CreatedAt <= to.Value);

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
            limit);
    }

    public async Task<PagedResult<SaleListItemDto>> ListDetailsAsync(
        int page,
        int limit,
        SaleStatus? status,
        int? userId,
        int? customerId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Sales
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Customer)
            .Where(s => s.IsDeleted != true);

        if (status.HasValue)
            query = query.Where(s => s.Status == (short)status.Value);

        if (userId.HasValue)
            query = query.Where(s => s.UserId == userId.Value);

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);

        if (from.HasValue)
            query = query.Where(s => s.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(s => s.CreatedAt <= to.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(s => new SaleListItemDto(
                s.SaleId,
                s.UserId,
                s.User != null ? s.User.UserName : null,
                s.CustomerId,
                s.Customer != null ? s.Customer.Name : null,
                (SaleStatus)(s.Status ?? (short)SaleStatus.DRAFT),
                s.TotalAmount ?? 0m,
                s.Discount ?? 0m,
                s.CreatedAt ?? DateTime.UtcNow,
                s.Note))
            .ToListAsync(cancellationToken);

        return new PagedResult<SaleListItemDto>(items, total, page, limit);
    }

    public async Task<SaleDetailsDto?> GetDetailsAsync(int saleId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Sales
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(i => i.Medicine)
            .Include(s => s.SaleItems)
                .ThenInclude(i => i.Batch)
            .Where(s => s.SaleId == saleId && s.IsDeleted != true)
            .Select(s => new SaleDetailsDto(
                s.SaleId,
                s.UserId,
                s.User != null ? s.User.UserName : null,
                s.CustomerId,
                s.Customer != null ? s.Customer.Name : null,
                (SaleStatus)(s.Status ?? (short)SaleStatus.DRAFT),
                s.TotalAmount ?? 0m,
                s.Discount ?? 0m,
                s.CreatedAt ?? DateTime.UtcNow,
                s.Note,
                s.SaleItems
                    .Where(i => i.IsDeleted != true)
                    .Select(i => new SaleItemDetailsDto(
                        i.SaleItemId,
                        i.MedicineId ?? 0,
                        i.Medicine != null ? i.Medicine.Name : null,
                        i.BatchId ?? 0,
                        i.Batch != null ? i.Batch.BatchNumber : null,
                        i.Quantity,
                        i.UnitPrice ?? 0m,
                        i.TotalPrice ?? 0m))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
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
            .Where(i => i.SaleId == saleId && i.IsDeleted != true)
            .ToListAsync(cancellationToken);

        return models.Select(MapItem).ToList();
    }

    public async Task UpdateTotalAmountAsync(int saleId, CancellationToken cancellationToken = default)
    {
        var sale = await _dbContext.Sales.FirstAsync(s => s.SaleId == saleId, cancellationToken);

        var itemsTotal = await _dbContext.SaleItems
            .AsNoTracking()
            .Where(i => i.SaleId == saleId && i.IsDeleted != true)
            .SumAsync(i => (decimal?)i.TotalPrice, cancellationToken) ?? 0m;

        sale.TotalAmount = itemsTotal;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UnpaidSaleDto>> GetUnpaidSalesByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var sales = await _dbContext.Sales.AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.IsDeleted != true && s.TotalAmount > 0)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        var saleIds = sales.Select(s => s.SaleId).ToList();
        
        var paidBySaleId = await _dbContext.Payments.AsNoTracking()
            .Where(p => p.ReferenceType == 1 && saleIds.Contains(p.ReferenceId) && p.IsDeleted != true)
            .GroupBy(p => p.ReferenceId)
            .Select(group => new { SaleId = group.Key, Total = group.Sum(p => p.Amount) })
            .ToDictionaryAsync(item => item.SaleId, item => item.Total, cancellationToken);

        return sales
            .Select(sale =>
            {
                var paid = paidBySaleId.TryGetValue(sale.SaleId, out var totalPaid) ? totalPaid : 0m;
                var totalAmount = sale.TotalAmount ?? 0m;
                return new UnpaidSaleDto(sale.SaleId, totalAmount, paid, totalAmount - paid, sale.CreatedAt ?? DateTime.UtcNow);
            })
            .Where(sale => sale.RemainingAmount > 0)
            .ToList();
    }

    public async Task<decimal> GetTotalSalesAmountByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Sales.AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.IsDeleted != true)
            .SumAsync(s => (decimal?)s.TotalAmount, cancellationToken) ?? 0m;
    }

    public async Task<IReadOnlyList<SaleEntity>> GetByCustomerIdAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Sales.AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.IsDeleted != true);

        if (from.HasValue) query = query.Where(s => s.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(s => s.CreatedAt <= to.Value);

        var models = await query.OrderBy(s => s.CreatedAt).ToListAsync(cancellationToken);
        return models.Select(Map).ToList();
    }

    public async Task<List<BatchStockInfo>> GetAvailableBatchesAsync(int medicineId, CancellationToken cancellationToken = default)
    {
        var batches = await _batchRepository.ListAvailableByMedicineAsync(medicineId, cancellationToken);

        return batches.Select(batch => new BatchStockInfo
            {
                BatchId = batch.BatchId,
                BatchNumber = batch.BatchNumber ?? string.Empty,
                QuantityRemaining = batch.QuantityRemaining,
                SellPrice = batch.SellPrice
            })
            .ToList();
    }

    public async Task<int> DecrementBatchStockAsync(int batchId, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            return 0;

        var batch = await _batchRepository.GetByIdAsync(batchId, cancellationToken);
        if (batch is null)
            return 0;

        try
        {
            batch.DecreaseStock(quantity);
            await _batchRepository.UpdateAsync(batch, cancellationToken);
            return 1;
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
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
