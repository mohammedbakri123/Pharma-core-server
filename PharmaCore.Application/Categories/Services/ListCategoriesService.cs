using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Services;

public class ListCategoriesService(ICategoryRepository categoryRepository, ILogger<ListCategoriesService> logger) : IListCategoriesService
{
    public async Task<ServiceResult<PagedResult<CategoryDto>>> ExecuteAsync(ListCategoriesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await categoryRepository.ListAsync(cancellationToken);


            var filtered = categories
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLowerInvariant();
                filtered = filtered.Where(c =>
                    c.Name.ToLowerInvariant().Contains(search) ||
                    (c.ArabicName != null && c.ArabicName.ToLowerInvariant().Contains(search)));
            }

            var total = filtered.Count();
            var items = filtered
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .Select(c => new CategoryDto(c.CategoryId, c.Name, c.ArabicName, c.IsDeleted))
                .ToList();

            return ServiceResult<PagedResult<CategoryDto>>.Ok(
                new PagedResult<CategoryDto>(items, total, query.Page, query.Limit));

        }
        catch (Exception e)
        {

            logger.LogError(e, "Error getting category list");
            string errMessage = $"Error getting category list, ${e.Message} , ${e.StackTrace} , ${e.Source}";
            return  ServiceResult<PagedResult<CategoryDto>>.Fail(ServiceErrorType.ServerError, errMessage);

        }
    }
}
