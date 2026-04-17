using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Abstractions.Persistence;
using SaleEntity = PharmaCore.Domain.Entities.Sale;
using SaleItemEntity = PharmaCore.Domain.Entities.SaleItem;

public interface ISaleRepository
{
    Task<SaleEntity?> GetByIdAsync(int saleId, CancellationToken cancellationToken = default);
    Task<SaleEntity?> GetByIdWithItemsAsync(int saleId, CancellationToken cancellationToken = default);
    Task<PagedResult<SaleEntity>> ListAsync(int page, int limit, SaleStatus? status, int? userId, int? customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<PagedResult<SaleListItemDto>> ListDetailsAsync(int page, int limit, SaleStatus? status, int? userId, int? customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<SaleDetailsDto?> GetDetailsAsync(int saleId, CancellationToken cancellationToken = default);
    Task<SaleEntity> AddAsync(SaleEntity sale, CancellationToken cancellationToken = default);
    Task<SaleEntity> UpdateAsync(SaleEntity sale, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int saleId, CancellationToken cancellationToken = default);

    Task<SaleItemEntity> AddItemAsync(SaleItemEntity item, CancellationToken cancellationToken = default);
    Task<SaleItemEntity?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default);
    Task<SaleItemEntity> UpdateItemAsync(SaleItemEntity item, CancellationToken cancellationToken = default);
    Task<bool> DeleteItemAsync(int itemId, CancellationToken cancellationToken = default);
    Task<List<SaleItemEntity>> GetItemsBySaleIdAsync(int saleId, CancellationToken cancellationToken = default);

    Task UpdateTotalAmountAsync(int saleId, CancellationToken cancellationToken = default);

    Task<List<BatchStockInfo>> GetAvailableBatchesAsync(int medicineId, CancellationToken cancellationToken = default);
    Task<int> DecrementBatchStockAsync(int batchId, int quantity, CancellationToken cancellationToken = default);
}

public class BatchStockInfo
{
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int QuantityRemaining { get; set; }
    public decimal SellPrice { get; set; }
}
