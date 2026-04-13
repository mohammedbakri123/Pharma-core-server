using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Application.Common.Pagination;

namespace PharmaCore.Application.Categories.Interfaces;

public interface IListCategoriesService
{
    Task<PagedResult<CategoryDto>> ExecuteAsync(ListCategoriesQuery query, CancellationToken cancellationToken = default);
}
