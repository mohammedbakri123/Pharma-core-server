using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Domain.Enums;
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
            .ToListAsync(cancellationToken);
        return models.Select(Map).ToList();
    }

    public async Task<PagedResult<UserEntity>> ListAsync(int page, int limit, UserRole? role, string? search, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users
            .AsNoTracking()
            .Where(user => user.IsDeleted != true);

        if (role.HasValue)
        {
            var roleValue = (short)role.Value;
            query = query.Where(user => user.Role == roleValue);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = $"%{search.Trim()}%";
            query = query.Where(user =>
                EF.Functions.ILike(user.UserName, normalizedSearch) ||
                (user.PhoneNumber != null && EF.Functions.ILike(user.PhoneNumber, normalizedSearch)));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(user => user.UserId)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<UserEntity>(items.Select(Map).ToList(), total, page, limit);
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
        model.DeletedAt = NormalizeTimestamp(user.DeletedAt);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE users SET is_deleted = TRUE, deleted_at = NOW() WHERE user_id = {userId} AND is_deleted IS NOT TRUE",
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

    private static DateTime? NormalizeTimestamp(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified);
    }
}
