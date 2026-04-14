using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Interfaces;

public interface IUpdateCategoryService
{
    Task<ServiceResult<CategoryDto>> ExecuteAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default);
}
