using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Interfaces;

public interface IListCategoriesService
{
    Task<ServiceResult<PagedResult<CategoryDto>>> ExecuteAsync(ListCategoriesQuery query, CancellationToken cancellationToken = default);
}
