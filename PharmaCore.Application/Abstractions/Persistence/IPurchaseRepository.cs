using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PurchaseEntity = PharmaCore.Domain.Entities.Purchase;
using PurchaseItemEntity = PharmaCore.Domain.Entities.PurchaseItem;

namespace PharmaCore.Application.Abstractions.Persistence;

public interface IPurchaseRepository
{
    Task<PurchaseEntity?> GetByIdAsync(int purchaseId, CancellationToken cancellationToken = default);
    Task<PurchaseEntity?> GetByIdWithItemsAsync(int purchaseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseEntity>> ListAsync(CancellationToken cancellationToken = default);
    Task<PurchaseEntity> AddAsync(PurchaseEntity purchase, CancellationToken cancellationToken = default);
    Task<PurchaseEntity> UpdateAsync(PurchaseEntity purchase, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int purchaseId, CancellationToken cancellationToken = default);
    Task<PurchaseItemEntity> AddItemAsync(PurchaseItemEntity item, CancellationToken cancellationToken = default);
    Task<PurchaseItemEntity?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default);
    Task<PurchaseItemEntity> UpdateItemAsync(PurchaseItemEntity item, CancellationToken cancellationToken = default);
    Task<bool> DeleteItemAsync(int itemId, CancellationToken cancellationToken = default);
    Task<List<PurchaseItemEntity>> GetItemsByPurchaseIdAsync(int purchaseId, CancellationToken cancellationToken = default);
    Task UpdateTotalAmountAsync(int purchaseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseEntity>> GetBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalPurchasesAmountBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default);
}