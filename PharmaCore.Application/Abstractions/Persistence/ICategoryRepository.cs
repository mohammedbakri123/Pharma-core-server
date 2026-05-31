using PharmaCore.Application.Common.Pagination;
using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Abstractions.Persistence;
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<PagedResult<Category>> ListAsync(
        string? search,
        int page,
        int limit,
        CancellationToken cancellationToken = default);  
    Task<IEnumerable<Category>> ListDeletedAsync(CancellationToken cancellationToken = default);
    Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);
    Task<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<bool> RestoreDeletedAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<bool> HardDeleteAsync(int categoryId, CancellationToken cancellationToken = default);
}