using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Requests;

namespace PharmaCore.Application.Categories.Interfaces;

public interface IGetCategoryByIdService
{
    Task<CategoryDto?> ExecuteAsync(GetCategoryByIdQuery query, CancellationToken cancellationToken = default);
}
