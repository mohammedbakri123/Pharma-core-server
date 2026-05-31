using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Entities;
using PharmaCore.Infrastructure.Utilities;
using CategoryEntity = PharmaCore.Domain.Entities.Category;
using CategoryModel = PharmaCore.Infrastructure.Models.Category;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class CategoryRepository(ApplicationDbContext dbContext) : ICategoryRepository
{
    public async Task<CategoryEntity?> GetByIdAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.CategoryId == categoryId && category.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    public async Task<IEnumerable<CategoryEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Categories
            .AsNoTracking()
            .Where(c => c.IsDeleted != true)
            .ToListAsync(cancellationToken);
        return models.Select(Map).ToList();
    }

    public async Task<IEnumerable<CategoryEntity>> ListDeletedAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Categories
            .AsNoTracking()
            .Where(c => c.IsDeleted == true)
            .ToListAsync(cancellationToken);
        return models.Select(Map).ToList();
    }

    public async Task<CategoryEntity> AddAsync(CategoryEntity category, CancellationToken cancellationToken = default)
    {
        var model = new CategoryModel
        {
            CategoryName = category.Name,
            CategoryArabicName = category.ArabicName,
            // CreatedAt = DateTimeHelper.NormalizeTimestamp(DateTime.UtcNow) ?? DateTime.UtcNow,
            IsDeleted = false
        };

        dbContext.Categories.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<CategoryEntity> UpdateAsync(CategoryEntity category, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Categories
            .FirstAsync(entity => entity.CategoryId == category.CategoryId, cancellationToken);

        model.CategoryName = category.Name;
        model.CategoryArabicName = category.ArabicName;
        model.IsDeleted = category.IsDeleted;
        model.DeletedAt = DateTimeHelper.NormalizeTimestamp(category.DeletedAt);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var deletedAt = DateTimeHelper.GetCurrentTimestamp();
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE categories SET is_deleted = TRUE, deleted_at = NOW() WHERE category_id = {categoryId} AND is_deleted IS NOT TRUE",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task<bool> RestoreDeletedAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE categories SET is_deleted = FALSE, deleted_at = NULL WHERE category_id = {categoryId} AND is_deleted IS TRUE",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task<bool> HardDeleteAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM categories WHERE category_id = {categoryId}",
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

}
     