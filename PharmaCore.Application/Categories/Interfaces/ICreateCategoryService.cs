using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Application.Common.Pagination;

namespace PharmaCore.Application.Categories.Interfaces;

public interface ICreateCategoryService
{
    Task<CategoryDto> ExecuteAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default);
}
