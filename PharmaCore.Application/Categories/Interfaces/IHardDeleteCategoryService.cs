using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Interfaces;

public interface IHardDeleteCategoryService
{
    Task<ServiceResult<bool>> ExecuteAsync(int categoryId, CancellationToken cancellationToken = default);
}