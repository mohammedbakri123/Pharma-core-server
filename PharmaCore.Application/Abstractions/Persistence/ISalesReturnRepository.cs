using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Abstractions.Persistence;

using SalesReturnEntity = PharmaCore.Domain.Entities.SalesReturn;
using SalesReturnItemEntity = PharmaCore.Domain.Entities.SalesReturnItem;

public interface ISalesReturnRepository
{
    Task<SalesReturnEntity?> GetByIdAsync(int salesReturnId, CancellationToken cancellationToken = default);
    Task<SalesReturnEntity?> GetByIdWithItemsAsync(int salesReturnId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SalesReturnEntity>> ListAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SalesReturnListItemDto>> ListDetailsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SalesReturnEntity>> GetByCustomerIdAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<SalesReturnDetailsDto?> GetDetailsAsync(int salesReturnId, CancellationToken cancellationToken = default);
    Task<SalesReturnEntity> AddAsync(SalesReturnEntity salesReturn, CancellationToken cancellationToken = default);
    Task<SalesReturnEntity> UpdateAsync(SalesReturnEntity salesReturn, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int salesReturnId, CancellationToken cancellationToken = default);

    Task<SalesReturnItemEntity> AddItemAsync(SalesReturnItemEntity item, CancellationToken cancellationToken = default);
    Task<SalesReturnItemEntity?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default);
    Task<List<SalesReturnItemEntity>> GetItemsBySalesReturnIdAsync(int salesReturnId, CancellationToken cancellationToken = default);

    Task UpdateTotalAmountAsync(int salesReturnId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalAmountByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
}