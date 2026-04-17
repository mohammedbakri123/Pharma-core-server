using PharmaCore.Application.Common.Pagination;
using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Abstractions.Persistence;
using SaleEntity = PharmaCore.Domain.Entities.Sale;

public interface ISaleRepository
{
    
    Task<SaleEntity?> GetByIdAsync(int saleId, CancellationToken cancellationToken = default);
    Task<PagedResult<SaleEntity>> ListAsync(int page, int limit, SaleStatus? status, int? userId,bool? isForWalkInCustomer, CancellationToken cancellationToken = default);
    Task<SaleEntity> AddAsync(SaleEntity sale, CancellationToken cancellationToken = default);
    Task<SaleEntity> UpdateAsync(SaleEntity sale, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int saleId, CancellationToken cancellationToken = default);
    Task<bool> HardDeleteAsync(int saleId, CancellationToken cancellationToken = default);
}