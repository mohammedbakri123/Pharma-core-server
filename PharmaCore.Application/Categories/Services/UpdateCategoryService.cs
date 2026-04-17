using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Services;

public class UpdateCategoryService : IUpdateCategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<UpdateCategoryService> _logger;

    public UpdateCategoryService(ICategoryRepository categoryRepository, ILogger<UpdateCategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<CategoryDto>> ExecuteAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);

        if (category == null)
        {
            return ServiceResult<CategoryDto>.Fail(ServiceErrorType.NotFound, $"Category with ID {command.CategoryId} not found.");
        }

        if (command.CategoryName != null)
            category.ChangeName(command.CategoryName);
        if (command.CategoryArabicName != null)
            category.ChangeArabicName(command.CategoryArabicName);

        var updated = await _categoryRepository.UpdateAsync(category, cancellationToken);

        _logger.LogInformation("Category '{CategoryName}' updated successfully with ID {CategoryId}", updated.Name, updated.CategoryId);

        return ServiceResult<CategoryDto>.Ok(
            new CategoryDto(updated.CategoryId, updated.Name, updated.ArabicName, updated.IsDeleted));
    }
}
