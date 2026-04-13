using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Categories.Services;

public class GetCategoryByIdService : IGetCategoryByIdService
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDto?> ExecuteAsync(GetCategoryByIdQuery query, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(query.CategoryId, cancellationToken);

        if (category == null || category.IsDeleted)
        {
            return null;
        }

        return new CategoryDto(category.CategoryId, category.CategoryName, category.CategoryArabicName);
    }
}
