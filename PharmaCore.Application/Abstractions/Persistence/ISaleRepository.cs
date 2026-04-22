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
    Task<IEnumerable<SaleEntity>> ListAsync(CancellationToken cancellationToken = default);
    Task<SaleEntity?> GetDetailsAsync(int saleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SaleEntity>> ListDetailsAsync(CancellationToken cancellationToken = default);
    Task<SaleEntity> AddAsync(SaleEntity sale, CancellationToken cancellationToken = default);
    Task<SaleEntity> UpdateAsync(SaleEntity sale, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int saleId, CancellationToken cancellationToken = default);
    Task<SaleItemEntity> AddItemAsync(SaleItemEntity item, CancellationToken cancellationToken = default);
    Task<SaleItemEntity?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default);
    Task<SaleItemEntity> UpdateItemAsync(SaleItemEntity item, CancellationToken cancellationToken = default);
    Task<bool> DeleteItemAsync(int itemId, CancellationToken cancellationToken = default);
    Task<List<SaleItemEntity>> GetItemsBySaleIdAsync(int saleId, CancellationToken cancellationToken = default);
    Task UpdateTotalAmountAsync(int saleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UnpaidSaleDto>> GetUnpaidSalesByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalSalesAmountByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SaleEntity>> GetByCustomerIdAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
