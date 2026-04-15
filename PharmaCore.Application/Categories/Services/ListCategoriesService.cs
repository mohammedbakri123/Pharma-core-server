using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Services;

public class ListCategoriesService : IListCategoriesService
{
    private readonly ICategoryRepository _categoryRepository;

    public ListCategoriesService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<ServiceResult<PagedResult<CategoryDto>>> ExecuteAsync(ListCategoriesQuery query, CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.ListAsync(cancellationToken);

        var filtered = categories
            .Where(c => !c.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLowerInvariant();
            filtered = filtered.Where(c =>
                c.CategoryName.ToLowerInvariant().Contains(search) ||
                (c.CategoryArabicName.ToLowerInvariant().Contains(search)));
        }

        var total = filtered.Count();
        var items = filtered
            .Skip((query.Page - 1) * query.Limit)
            .Take(query.Limit)
            .Select(c => new CategoryDto(c.CategoryId, c.CategoryName, c.CategoryArabicName, c.IsDeleted))
            .ToList();

        return ServiceResult<PagedResult<CategoryDto>>.Ok(
            new PagedResult<CategoryDto>(items, total, query.Page, query.Limit));
    }
}
