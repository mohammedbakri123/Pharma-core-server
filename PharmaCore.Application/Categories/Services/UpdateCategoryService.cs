using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Services;

public class UpdateCategoryService(ICategoryRepository categoryRepository, ILogger<UpdateCategoryService> logger)
    : IUpdateCategoryService
{
    public async Task<ServiceResult<CategoryDto>> ExecuteAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);

            if (category == null)
            {
                return ServiceResult<CategoryDto>.Fail(ServiceErrorType.NotFound,
                    $"Category with ID {command.CategoryId} not found.");
            }

            if (command.CategoryName != null)
                category.ChangeName(command.CategoryName);
            if (command.CategoryArabicName != null)
                category.ChangeArabicName(command.CategoryArabicName);

            var updated = await categoryRepository.UpdateAsync(category, cancellationToken);

            logger.LogInformation("Category '{CategoryName}' updated successfully with ID {CategoryId}", updated.Name,
                updated.CategoryId);

            return ServiceResult<CategoryDto>.Ok(
                new CategoryDto(updated.CategoryId, updated.Name, updated.ArabicName, updated.IsDeleted));

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating category");
            string errMessage = $"Error updating category, ${e.Message} , ${e.StackTrace} , ${e.Source}";
            return ServiceResult<CategoryDto>.Fail(ServiceErrorType.ServerError, errMessage);
        }
    }
}
