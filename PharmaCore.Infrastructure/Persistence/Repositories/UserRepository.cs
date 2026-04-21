using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Enums;
using PharmaCore.Infrastructure.Utilities;
using UserEntity = PharmaCore.Domain.Entities.User;
using UserModel = PharmaCore.Infrastructure.Models.User;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserEntity?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.UserId == userId && user.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    public async Task<UserEntity?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var normalized = userName.Trim();

        var model = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.IsDeleted != true && user.UserName.ToLower() == normalized.ToLower(),
                cancellationToken);

        return model is null ? null : Map(model);
    }

    public Task<bool> UserNameExistsAsync(string userName, int? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var normalized = userName.Trim().ToLower();

        return _dbContext.Users.AnyAsync(
            user => user.IsDeleted != true
                && user.UserName.ToLower() == normalized
                && (!excludeUserId.HasValue || user.UserId != excludeUserId.Value),
            cancellationToken);
    }

    public async Task<IEnumerable<UserEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.IsDeleted != true)
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
            CreatedAt = DateTimeHelper.NormalizeTimestamp(DateTime.UtcNow) ?? DateTime.UtcNow,
            IsDeleted = false
        };

        _dbContext.Users.Add(model);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<UserEntity> UpdateAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Users.FirstAsync(entity => entity.UserId == user.UserId, cancellationToken);

        model.UserName = user.UserName;
        model.PasswordHash = user.PasswordHash;
        model.PhoneNumber = user.PhoneNumber;
        model.Address = user.Address;
        model.Role = (short)user.Role;
        model.IsDeleted = user.IsDeleted;
        model.DeletedAt = DateTimeHelper.NormalizeTimestamp(user.DeletedAt);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var deletedAt = DateTimeHelper.NormalizeTimestamp(DateTime.UtcNow);
        var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE users SET is_deleted = TRUE, deleted_at = {deletedAt} WHERE user_id = {userId} AND is_deleted IS NOT TRUE",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task<bool> HardDeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
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
