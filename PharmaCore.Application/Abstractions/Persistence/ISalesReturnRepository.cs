using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Abstractions.Persistence;

using SalesReturnEntity = PharmaCore.Domain.Entities.SalesReturn;
using SalesReturnItemEntity = PharmaCore.Domain.Entities.SalesReturnItem;

public interface ISalesReturnRepository
{
    Task<SalesReturnEntity?> GetByIdAsync(int salesReturnId, CancellationToken cancellationToken = default);
    Task<SalesReturnEntity?> GetByIdWithItemsAsync(int salesReturnId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SalesReturnEntity>> ListAsync(CancellationToken cancellationToken = default);
    
    Task<PagedResult<SalesReturnEntity>> ListPagedAsync(ListSalesReturnQuery query,CancellationToken cancellationToken = default);

    Task<IEnumerable<SalesReturnEntity>> ListDetailsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SalesReturnEntity>> GetByCustomerIdAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<IEnumerable<SalesReturnEntity>> GetBySaleIdWithItemsAsync(int saleId,SalesReturnStatus? status, CancellationToken cancellationToken = default);

    Task<SalesReturnEntity?> GetDetailsAsync(int salesReturnId, CancellationToken cancellationToken = default);
    Task<SalesReturnEntity> AddAsync(SalesReturnEntity salesReturn, CancellationToken cancellationToken = default);
    Task<SalesReturnEntity> UpdateAsync(SalesReturnEntity salesReturn, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int salesReturnId, CancellationToken cancellationToken = default);

    Task<SalesReturnItemEntity> AddItemAsync(SalesReturnItemEntity item, CancellationToken cancellationToken = default);
    Task<SalesReturnItemEntity?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default);
    Task<List<SalesReturnItemEntity>> GetItemsBySalesReturnIdAsync(int salesReturnId, CancellationToken cancellationToken = default);
    Task<SalesReturnItemEntity> UpdateItemAsync(SalesReturnItemEntity item, CancellationToken cancellationToken = default);
    Task<bool> DeleteItemAsync(int itemId, CancellationToken cancellationToken = default);

    Task<bool> ExistsDraftForSaleAsync(int saleId, CancellationToken cancellationToken = default);
    Task<int> GetCompletedReturnQuantityBySaleItemAsync(int saleItemId, CancellationToken cancellationToken = default);

    Task UpdateTotalAmountAsync(int salesReturnId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalAmountByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalAmountBySaleIdAsync(int saleId, CancellationToken cancellationToken = default);
}