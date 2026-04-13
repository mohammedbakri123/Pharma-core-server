using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Requests;

namespace PharmaCore.Application.Categories.Interfaces;

public interface IUpdateCategoryService
{
    Task<CategoryDto> ExecuteAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default);
}
