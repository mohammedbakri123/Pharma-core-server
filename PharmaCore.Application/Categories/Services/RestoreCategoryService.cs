using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Services;

public class RestoreCategoryService(ICategoryRepository categoryRepository, ILogger<RestoreCategoryService> logger)
    : IRestoreCategoryService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(RestoreCategoryCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await categoryRepository.RestoreDeletedAsync(command.CategoryId, cancellationToken);

            if (!result)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Deleted category with ID {command.CategoryId} not found or is not deleted.");
            }

            logger.LogInformation("Category with ID {CategoryId} restored successfully", command.CategoryId);

            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error restoring category");
            string errMessage = $"Error restoring category, {e.Message} , {e.StackTrace} , {e.Source}";
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, errMessage);
        }
    }
}
