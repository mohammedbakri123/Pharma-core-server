using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Interfaces;

public interface ICreateCategoryService
{
    Task<ServiceResult<CategoryDto>> ExecuteAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default);
}
