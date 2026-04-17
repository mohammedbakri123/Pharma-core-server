using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class SaleRepository : ISaleRepository
{
    public Task<Sale?> GetByIdAsync(int saleId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResult<Sale>> ListAsync(int page, int limit, SaleStatus? status, int? userId, bool? isForWalkInCustomer,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Sale> AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteAsync(int saleId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HardDeleteAsync(int saleId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}