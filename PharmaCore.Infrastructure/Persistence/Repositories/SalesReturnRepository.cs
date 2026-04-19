using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Domain.Entities;
using SalesReturnEntity = PharmaCore.Domain.Entities.SalesReturn;
using SalesReturnItemEntity = PharmaCore.Domain.Entities.SalesReturnItem;
using SalesReturnModel = PharmaCore.Infrastructure.Models.SalesReturn;
using SalesReturnItemModel = PharmaCore.Infrastructure.Models.SalesReturnItem;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class SalesReturnRepository : ISalesReturnRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IBatchRepository _batchRepository;

    public SalesReturnRepository(ApplicationDbContext dbContext, IBatchRepository batchRepository)
    {
        _dbContext = dbContext;
        _batchRepository = batchRepository;
    }

    public async Task<SalesReturnEntity?> GetByIdAsync(int salesReturnId, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.SalesReturns
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.SalesReturnId == salesReturnId && r.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    public async Task<SalesReturnEntity?> GetByIdWithItemsAsync(int salesReturnId, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.SalesReturns
            .AsNoTracking()
            .Include(r => r.SalesReturnItems)
            .FirstOrDefaultAsync(r => r.SalesReturnId == salesReturnId && r.IsDeleted != true, cancellationToken);

        return model is null ? null : MapWithItems(model);
    }

    public async Task<IEnumerable<SalesReturnEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await _dbContext.SalesReturns
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return models.Select(Map).ToList();
    }

    public async Task<PagedResult<SalesReturnEntity>> ListAsync(
        int page,
        int limit,
        int? saleId,
        int? customerId,
        int? userId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SalesReturns
            .AsNoTracking()
            .Where(r => r.IsDeleted != true);

        if (saleId.HasValue)
            query = query.Where(r => r.SaleId == saleId.Value);

        if (customerId.HasValue)
            query = query.Where(r => r.CustomerId == customerId.Value);

        if (userId.HasValue)
            query = query.Where(r => r.UserId == userId.Value);

        if (from.HasValue)
            query = query.Where(r => r.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(r => r.CreatedAt <= to.Value);

        var total = await query.CountAsync(cancellationToken);

        var models = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<SalesReturnEntity>(
            models.Select(Map).ToList(),
            total,
            page,
            limit);
    }

    public async Task<PagedResult<SalesReturnListItemDto>> ListDetailsAsync(
        int page,
        int limit,
        int? saleId,
        int? customerId,
        int? userId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SalesReturns
            .AsNoTracking()
            .Include(r => r.Sale)
            .Include(r => r.Customer)
            .Include(r => r.User)
            .Where(r => r.IsDeleted != true);

        if (saleId.HasValue)
            query = query.Where(r => r.SaleId == saleId.Value);

        if (customerId.HasValue)
            query = query.Where(r => r.CustomerId == customerId.Value);

        if (userId.HasValue)
            query = query.Where(r => r.UserId == userId.Value);

        if (from.HasValue)
            query = query.Where(r => r.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(r => r.CreatedAt <= to.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(r => new SalesReturnListItemDto(
                r.SalesReturnId,
                r.SaleId,
                r.Sale != null ? r.Sale.SaleId.ToString() : null,
                r.CustomerId,
                r.Customer != null ? r.Customer.Name : null,
                r.UserId,
                r.User != null ? r.User.UserName : null,
                r.TotalAmount ?? 0m,
                r.Note,
                r.CreatedAt ?? DateTime.UtcNow))
            .ToListAsync(cancellationToken);

        return new PagedResult<SalesReturnListItemDto>(items, total, page, limit);
    }

    public async Task<SalesReturnDetailsDto?> GetDetailsAsync(int salesReturnId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SalesReturns
            .AsNoTracking()
            .Include(r => r.Sale)
            .Include(r => r.Customer)
            .Include(r => r.User)
            .Include(r => r.SalesReturnItems)
                .ThenInclude(i => i.Batch)
            .Where(r => r.SalesReturnId == salesReturnId && r.IsDeleted != true)
            .Select(r => new SalesReturnDetailsDto(
                r.SalesReturnId,
                r.SaleId,
                r.Sale != null ? r.Sale.SaleId.ToString() : null,
                r.CustomerId,
                r.Customer != null ? r.Customer.Name : null,
                r.UserId,
                r.User != null ? r.User.UserName : null,
                r.TotalAmount ?? 0m,
                r.Note,
                r.CreatedAt ?? DateTime.UtcNow,
                r.SalesReturnItems
                    .Where(i => i.IsDeleted != true)
                    .Select(i => new SalesReturnItemDetailsDto(
                        i.SalesReturnItemId,
                        i.SaleItemId ?? 0,
                        i.BatchId ?? 0,
                        i.Batch != null ? i.Batch.BatchNumber : null,
                        i.Quantity ?? 0,
                        i.UnitPrice ?? 0m,
                        i.TotalPrice ?? 0m))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SalesReturnEntity> AddAsync(SalesReturnEntity salesReturn, CancellationToken cancellationToken = default)
    {
        var model = new SalesReturnModel
        {
            SaleId = salesReturn.SaleId,
            CustomerId = salesReturn.CustomerId,
            UserId = salesReturn.UserId,
            TotalAmount = salesReturn.TotalAmount,
            Note = salesReturn.Note,
            CreatedAt = salesReturn.CreatedAt,
            IsDeleted = false
        };

        _dbContext.SalesReturns.Add(model);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<SalesReturnEntity> UpdateAsync(SalesReturnEntity salesReturn, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.SalesReturns
            .FirstAsync(r => r.SalesReturnId == salesReturn.SalesReturnId, cancellationToken);

        model.SaleId = salesReturn.SaleId;
        model.CustomerId = salesReturn.CustomerId;
        model.UserId = salesReturn.UserId;
        model.TotalAmount = salesReturn.TotalAmount;
        model.Note = salesReturn.Note;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int salesReturnId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE sales_returns SET is_deleted = TRUE, deleted_at = NOW() WHERE sales_return_id = {salesReturnId} AND is_deleted IS NOT TRUE",
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

        _dbContext.SalesReturnItems.Add(model);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapItem(model);
    }

    public async Task<SalesReturnItemEntity?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.SalesReturnItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.SalesReturnItemId == itemId, cancellationToken);

        return model is null ? null : MapItem(model);
    }

    public async Task<List<SalesReturnItemEntity>> GetItemsBySalesReturnIdAsync(int salesReturnId, CancellationToken cancellationToken = default)
    {
        var models = await _dbContext.SalesReturnItems
            .AsNoTracking()
            .Where(i => i.SalesReturnId == salesReturnId && i.IsDeleted != true)
            .ToListAsync(cancellationToken);

        return models.Select(MapItem).ToList();
    }

    public async Task UpdateTotalAmountAsync(int salesReturnId, CancellationToken cancellationToken = default)
    {
        var salesReturn = await _dbContext.SalesReturns.FirstAsync(r => r.SalesReturnId == salesReturnId, cancellationToken);

        var itemsTotal = await _dbContext.SalesReturnItems
            .AsNoTracking()
            .Where(i => i.SalesReturnId == salesReturnId && i.IsDeleted != true)
            .SumAsync(i => (decimal?)i.TotalPrice, cancellationToken) ?? 0m;

        salesReturn.TotalAmount = itemsTotal;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> IncrementBatchStockAsync(int batchId, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            return 0;

        var batch = await _batchRepository.GetByIdAsync(batchId, cancellationToken);
        if (batch is null)
            return 0;

        try
        {
            batch.IncreaseStock(quantity);
            await _batchRepository.UpdateAsync(batch, cancellationToken);
            return 1;
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
    }

    public async Task<decimal> GetTotalAmountByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SalesReturns.AsNoTracking()
            .Where(r => r.CustomerId == customerId && r.IsDeleted != true)
            .SumAsync(r => (decimal?)r.TotalAmount, cancellationToken) ?? 0m;
    }

    private static SalesReturnEntity Map(SalesReturnModel model)
    {
        return SalesReturnEntity.Rehydrate(
            model.SalesReturnId,
            model.SaleId,
            model.CustomerId,
            model.UserId,
            model.TotalAmount ?? 0m,
            model.Note,
            model.CreatedAt ?? DateTime.UtcNow,
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
            model.TotalAmount ?? 0m,
            model.Note,
            model.CreatedAt ?? DateTime.UtcNow,
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