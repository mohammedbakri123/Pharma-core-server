using PharmaCore.Domain.Entities;
using PurchaseReturnEntity = PharmaCore.Domain.Entities.PurchaseReturn;
using PurchaseReturnItemEntity = PharmaCore.Domain.Entities.PurchaseReturnItem;

namespace PharmaCore.Application.Abstractions.Persistence;

public interface IPurchaseReturnRepository
{
    Task<PurchaseReturnEntity?> GetByIdAsync(int purchaseReturnId, CancellationToken cancellationToken = default);
    Task<PurchaseReturnEntity?> GetByIdWithItemsAsync(int purchaseReturnId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseReturnEntity>> ListAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseReturnEntity>> GetBySupplierIdAsync(int supplierId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<PurchaseReturnEntity> AddAsync(PurchaseReturnEntity purchaseReturn, CancellationToken cancellationToken = default);
    Task<PurchaseReturnEntity> UpdateAsync(PurchaseReturnEntity purchaseReturn, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int purchaseReturnId, CancellationToken cancellationToken = default);

    Task<PurchaseReturnItemEntity> AddItemAsync(PurchaseReturnItemEntity item, CancellationToken cancellationToken = default);
    Task<PurchaseReturnItemEntity?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default);
    Task<List<PurchaseReturnItemEntity>> GetItemsByPurchaseReturnIdAsync(int purchaseReturnId, CancellationToken cancellationToken = default);
    Task<PurchaseReturnItemEntity> UpdateItemAsync(PurchaseReturnItemEntity item, CancellationToken cancellationToken = default);
    Task<bool> DeleteItemAsync(int itemId, CancellationToken cancellationToken = default);

    Task UpdateTotalAmountAsync(int purchaseReturnId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalAmountBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default);
}