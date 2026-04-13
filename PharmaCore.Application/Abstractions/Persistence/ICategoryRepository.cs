using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Abstractions.Persistence;
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> ListAsync(CancellationToken cancellationToken = default);
    Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);
    Task<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int categoryId, CancellationToken cancellationToken = default);
}