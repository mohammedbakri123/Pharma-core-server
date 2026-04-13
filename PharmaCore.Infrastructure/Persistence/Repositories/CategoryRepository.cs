using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Entities;
using CategoryEntity = PharmaCore.Domain.Entities.Category;
using CategoryModel = PharmaCore.Infrastructure.Models.Category;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CategoryEntity?> GetByIdAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.CategoryId == categoryId && category.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    public async Task<IEnumerable<CategoryEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.IsDeleted != true)
            .OrderBy(category => category.CategoryName)
            .ToListAsync(cancellationToken);

        return models.Select(Map).ToList();
    }

    public async Task<CategoryEntity> AddAsync(CategoryEntity category, CancellationToken cancellationToken = default)
    {
        var model = new CategoryModel
        {
            CategoryName = category.CategoryName,
            CategoryArabicName = category.CategoryArabicName,
            IsDeleted = false
        };

        _dbContext.Categories.Add(model);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<CategoryEntity> UpdateAsync(CategoryEntity category, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Categories
            .FirstAsync(entity => entity.CategoryId == category.CategoryId, cancellationToken);

        model.CategoryName = category.CategoryName;
        model.CategoryArabicName = category.CategoryArabicName;
        model.IsDeleted = category.IsDeleted;
        model.DeletedAt = NormalizeTimestamp(category.DeletedAt);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE categories SET is_deleted = TRUE, deleted_at = NOW() WHERE category_id = {categoryId} AND is_deleted IS NOT TRUE",
            cancellationToken);

        return affectedRows > 0;
    }

    private static CategoryEntity Map(CategoryModel model)
    {
        return CategoryEntity.Rehydrate(
            model.CategoryId,
            model.CategoryName,
            model.CategoryArabicName,
            model.IsDeleted ?? false,
            model.DeletedAt);
    }

    private static DateTime? NormalizeTimestamp(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified);
    }
}
     