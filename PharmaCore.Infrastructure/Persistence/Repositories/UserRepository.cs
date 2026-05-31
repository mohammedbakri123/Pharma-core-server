using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Enums;
using PharmaCore.Infrastructure.Utilities;
using UserEntity = PharmaCore.Domain.Entities.User;
using UserModel = PharmaCore.Infrastructure.Models.User;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public async Task<UserEntity?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.UserId == userId && user.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    public async Task<UserEntity?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var normalized = userName.Trim();

        var model = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.IsDeleted != true && user.UserName.ToLower() == normalized.ToLower(),
                cancellationToken);

        return model is null ? null : Map(model);
    }

    public Task<bool> UserNameExistsAsync(string userName, int? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var normalized = userName.Trim().ToLower();

        return dbContext.Users.AnyAsync(
            user => user.IsDeleted != true
                && user.UserName.ToLower() == normalized
                && (!excludeUserId.HasValue || user.UserId != excludeUserId.Value),
            cancellationToken);
    }

    public async Task<IEnumerable<UserEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.IsDeleted != true)
            .ToListAsync(cancellationToken);
        return models.Select(Map).ToList();
    }

    public async Task<IEnumerable<UserEntity>> ListDeletedAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.IsDeleted == true)
            .ToListAsync(cancellationToken);

        return models.Select(Map).ToList();
    }

    public async Task<UserEntity> AddAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        var model = new UserModel
        {
            UserName = user.UserName,
            PasswordHash = user.PasswordHash,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            Role = (short)user.Role,
            CreatedAt = DateTimeHelper.GetCurrentTimestamp(),
            IsDeleted = false
        };

        dbContext.Users.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<UserEntity> UpdateAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Users.FirstAsync(entity => entity.UserId == user.UserId, cancellationToken);

        model.UserName = user.UserName;
        model.PasswordHash = user.PasswordHash;
        model.PhoneNumber = user.PhoneNumber;
        model.Address = user.Address;
        model.Role = (short)user.Role;
        model.IsDeleted = user.IsDeleted;
        model.DeletedAt = DateTimeHelper.NormalizeTimestamp(user.DeletedAt);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var deletedAt = DateTimeHelper.GetCurrentTimestamp();
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE users SET is_deleted = TRUE, deleted_at = {deletedAt} WHERE user_id = {userId} AND is_deleted IS NOT TRUE",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task<bool> RestoreDeletedAsync(int userId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE users SET is_deleted = FALSE, deleted_at = NULL WHERE user_id = {userId} AND is_deleted IS TRUE",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task<bool> HardDeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM users WHERE user_id = {userId}",
            cancellationToken);

        return affectedRows > 0;
    }

    private static UserEntity Map(UserModel model)
    {
        return UserEntity.Rehydrate(
            model.UserId,
            model.UserName,
            model.PasswordHash,
            model.PhoneNumber,
            model.Address,
            (UserRole)model.Role,
            model.CreatedAt,
            model.IsDeleted ?? false,
            model.DeletedAt);
    }

}
