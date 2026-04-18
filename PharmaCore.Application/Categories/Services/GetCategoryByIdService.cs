using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Services;

public class GetCategoryByIdService(ICategoryRepository categoryRepository,  ILogger<GetCategoryByIdService> logger) : IGetCategoryByIdService
{
    public async Task<ServiceResult<CategoryDto>> ExecuteAsync(GetCategoryByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {

            var category = await categoryRepository.GetByIdAsync(query.CategoryId, cancellationToken);

            if (category == null || category.IsDeleted)
            {
                return ServiceResult<CategoryDto>.Fail(ServiceErrorType.NotFound, "Category not found.");
            }

            return ServiceResult<CategoryDto>.Ok(
                new CategoryDto(category.CategoryId, category.Name, category.ArabicName, category.IsDeleted));

        }
        catch (Exception e)
        {
        
            logger.LogError(e, "Error getting category by id");
            string errMessage = $"Error getting category by id, ${e.Message} , ${e.StackTrace} , ${e.Source}";
            return ServiceResult<CategoryDto>.Fail(ServiceErrorType.ServerError, errMessage);

        }
    }
}
