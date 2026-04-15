using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Categories.Services;

public class CreateCategoryService(ICategoryRepository categoryRepository, ILogger<CreateCategoryService> logger)
    : ICreateCategoryService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly ILogger<CreateCategoryService> _logger = logger;

    public async Task<ServiceResult<CategoryDto>> ExecuteAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.CategoryName))
        {
            return ServiceResult<CategoryDto>.Fail(ServiceErrorType.Validation, "Category name is required.");
        }

        var category = Category.Create(command.CategoryName, command.CategoryArabicName);

        var created = await _categoryRepository.AddAsync(category, cancellationToken);

        _logger.LogInformation("Category '{CategoryName}' created successfully with ID {CategoryId}", created.CategoryName, created.CategoryId);

        return ServiceResult<CategoryDto>.Ok(
            new CategoryDto(created.CategoryId, created.CategoryName, created.CategoryArabicName, created.IsDeleted));
    }
}
