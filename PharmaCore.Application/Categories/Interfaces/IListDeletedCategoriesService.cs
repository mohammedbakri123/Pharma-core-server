using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Interfaces;

public interface IListDeletedCategoriesService
{
    Task<ServiceResult<PagedResult<CategoryDto>>> ExecuteAsync(ListDeletedCategoriesQuery query, CancellationToken cancellationToken = default);
}
