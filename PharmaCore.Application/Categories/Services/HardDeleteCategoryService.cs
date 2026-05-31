using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Services;

public class HardDeleteCategoryService(
    ICategoryRepository categoryRepository,
    ILogger<HardDeleteCategoryService> logger)
    : IHardDeleteCategoryService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);

            if (category == null)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Category with ID {categoryId} not found.");
            }

            var result = await categoryRepository.HardDeleteAsync(categoryId, cancellationToken);

            if (!result)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, "Failed to permanently delete category.");
            }

            logger.LogInformation("Category with ID {Id} permanently deleted", categoryId);

            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error permanently deleting category {CategoryId}", categoryId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error permanently deleting category: {e.Message}");
        }
    }
}