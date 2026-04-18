using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Services;

public class DeleteCategoryService(ICategoryRepository categoryRepository, ILogger<DeleteCategoryService> logger)
    : IDeleteCategoryService
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

            category.MarkDeleted();

            var result = await categoryRepository.SoftDeleteAsync(categoryId, cancellationToken);

            if (!result)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, "Failed to delete category.");
            }

            logger.LogInformation("Category with ID {CategoryId} deleted successfully", categoryId);

            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting category");
            string errMessage = $"Error deleting category, ${e.Message} , ${e.StackTrace} , ${e.Source}";
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, errMessage);

        }
    }
}
