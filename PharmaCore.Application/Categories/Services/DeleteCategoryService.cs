using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Common.Exceptions;

namespace PharmaCore.Application.Categories.Services;

public class DeleteCategoryService : IDeleteCategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<DeleteCategoryService> _logger;

    public DeleteCategoryService(ICategoryRepository categoryRepository, ILogger<DeleteCategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task ExecuteAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException($"Category with ID {categoryId} not found.");
        }

        category.MarkDeleted();

        var result = await _categoryRepository.SoftDeleteAsync(categoryId, cancellationToken);

        if (!result)
        {
            throw new AppValidationException("Failed to delete category.");
        }

        _logger.LogInformation("Category with ID {CategoryId} deleted successfully", categoryId);
    }
}
