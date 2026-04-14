using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Services;

public class GetCategoryByIdService : IGetCategoryByIdService
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<ServiceResult<CategoryDto>> ExecuteAsync(GetCategoryByIdQuery query, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(query.CategoryId, cancellationToken);

        if (category == null || category.IsDeleted)
        {
            return ServiceResult<CategoryDto>.Fail(ServiceErrorType.NotFound, "Category not found.");
        }

        return ServiceResult<CategoryDto>.Ok(
            new CategoryDto(category.CategoryId, category.CategoryName, category.CategoryArabicName));
    }
}
