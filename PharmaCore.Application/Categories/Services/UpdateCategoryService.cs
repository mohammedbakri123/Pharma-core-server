using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Application.Common.Exceptions;
using PharmaCore.Domain.Entities;

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

    public async Task<CategoryDto> ExecuteAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException($"Category with ID {command.CategoryId} not found.");
        }

        category.Update(command.CategoryName, command.CategoryArabicName);

        var updated = await _categoryRepository.UpdateAsync(category, cancellationToken);

        _logger.LogInformation("Category '{CategoryName}' updated successfully with ID {CategoryId}", updated.CategoryName, updated.CategoryId);

        return new CategoryDto(updated.CategoryId, updated.CategoryName, updated.CategoryArabicName);
    }
}
