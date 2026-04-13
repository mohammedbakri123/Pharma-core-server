using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Application.Common.Exceptions;
using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Categories.Services;

public class CreateCategoryService : ICreateCategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CreateCategoryService> _logger;

    public CreateCategoryService(ICategoryRepository categoryRepository, ILogger<CreateCategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<CategoryDto> ExecuteAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.CategoryName))
        {
            throw new AppValidationException("Category name is required.");
        }

        var category = Category.Create(command.CategoryName, command.CategoryArabicName);

        var created = await _categoryRepository.AddAsync(category, cancellationToken);

        _logger.LogInformation("Category '{CategoryName}' created successfully with ID {CategoryId}", created.CategoryName, created.CategoryId);

        return new CategoryDto(created.CategoryId, created.CategoryName, created.CategoryArabicName);
    }
}
