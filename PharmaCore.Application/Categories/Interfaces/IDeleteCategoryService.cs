using PharmaCore.Application.Categories.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Interfaces;

public interface IDeleteCategoryService
{
    Task<ServiceResult<bool>> ExecuteAsync(int categoryId, CancellationToken cancellationToken = default);
}
