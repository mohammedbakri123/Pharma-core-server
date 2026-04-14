using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Interfaces;

public interface IGetCategoryByIdService
{
    Task<ServiceResult<CategoryDto>> ExecuteAsync(GetCategoryByIdQuery query, CancellationToken cancellationToken = default);
}
