using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Services;

//new constructor way
public class CreateCategoryService(ICategoryRepository categoryRepository, ILogger<CreateCategoryService> logger)
    : ICreateCategoryService
{
    public async Task<ServiceResult<CategoryDto>> ExecuteAsync(CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {

            if (string.IsNullOrWhiteSpace(command.CategoryName))
            {
                return ServiceResult<CategoryDto>.Fail(ServiceErrorType.Validation, "Category name is required.");
            }

            var category = Category.Create(command.CategoryName, command.CategoryArabicName);

            var created = await categoryRepository.AddAsync(category, cancellationToken);

            logger.LogInformation("Category '{CategoryName}' created successfully with ID {CategoryId}", created.Name, created.CategoryId);

            return ServiceResult<CategoryDto>.Ok(
                new CategoryDto(created.CategoryId, created.Name, created.ArabicName, created.IsDeleted));

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating category");
            string errMessage = $"Error creating category, ${e.Message} , ${e.StackTrace} , ${e.Source}";
            return ServiceResult<CategoryDto>.Fail(ServiceErrorType.ServerError, errMessage);
        }
    }
}
