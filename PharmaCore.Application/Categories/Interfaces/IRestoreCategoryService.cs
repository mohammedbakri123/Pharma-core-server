using PharmaCore.Application.Categories.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Interfaces;

public interface IRestoreCategoryService
{
    Task<ServiceResult<bool>> ExecuteAsync(RestoreCategoryCommand command, CancellationToken cancellationToken = default);
}
