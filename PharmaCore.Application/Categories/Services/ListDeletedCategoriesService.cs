using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Services;

public class ListDeletedCategoriesService(ICategoryRepository categoryRepository, ILogger<ListDeletedCategoriesService> logger) : IListDeletedCategoriesService
{
    public async Task<ServiceResult<PagedResult<CategoryDto>>> ExecuteAsync(ListDeletedCategoriesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await categoryRepository.ListDeletedAsync(
                query.Search,
                query.Page,
                query.Limit,
                cancellationToken);

            return ServiceResult<PagedResult<CategoryDto>>
                .Ok(MapToDto(result));
        }        catch (Exception e)
        {
            logger.LogError(e, "Error getting deleted category list");
            string errMessage = $"Error getting deleted category list, {e.Message} , {e.StackTrace} , {e.Source}";
            return ServiceResult<PagedResult<CategoryDto>>.Fail(ServiceErrorType.ServerError, errMessage);
        }
    }
    
    private static PagedResult<CategoryDto> MapToDto(PagedResult<Category> result)
    {
        var items = result.Items
            .Select(c => new CategoryDto(c.CategoryId, c.Name, c.ArabicName))
            .ToList();

        return new PagedResult<CategoryDto>(items, result.Total, result.Page, result.Limit);
    }
}


