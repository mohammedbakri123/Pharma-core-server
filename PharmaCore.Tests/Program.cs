using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Users.Requests;
using PharmaCore.Application.Users.Services;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;

var tests = new (string Name, Func<Task> Run)[]
{
    ("ListDeletedUsers filters by role and search", ListDeletedUsersFiltersByRoleAndSearch),
    ("RestoreUser returns not found for active or missing users", RestoreUserReturnsNotFoundForActiveOrMissingUsers),
    ("RestoreUser restores deleted user", RestoreUserRestoresDeletedUser),
};

var failures = new List<string>();

foreach (var test in tests)
{
    try
    {
        await test.Run();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception ex)
    {
        failures.Add($"{test.Name}: {ex.Message}");
        Console.WriteLine($"FAIL {test.Name}");
    }
}

if (failures.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine("Failures:");
    foreach (var failure in failures)
    {
        Console.WriteLine(failure);
    }

    Environment.ExitCode = 1;
}

static async Task ListDeletedUsersFiltersByRoleAndSearch()
{
    var repository = new InMemoryUserRepository(
    [
        User.Rehydrate(1, "admin-one", "hash", "111", null, UserRole.Admin, DateTime.UtcNow, true, DateTime.UtcNow),
        User.Rehydrate(2, "cashier-one", "hash", "222", null, UserRole.Cashier, DateTime.UtcNow, true, DateTime.UtcNow),
        User.Rehydrate(3, "active-admin", "hash", "333", null, UserRole.Admin, DateTime.UtcNow, false, null),
    ]);

    var service = new ListDeletedUsersService(repository, new TestLogger<ListDeletedUsersService>());
    var result = await service.ExecuteAsync(new ListDeletedUsersQuery(1, 20, (short)UserRole.Admin, "admin"));

    Assert(result.Success, "Expected successful result.");
    Assert(result.Data is not null, "Expected paged result.");
    Assert(result.Data!.Total == 1, $"Expected one matching deleted user, got {result.Data.Total}.");
    Assert(result.Data.Items[0].UserId == 1, "Expected deleted admin user.");
}

static async Task RestoreUserReturnsNotFoundForActiveOrMissingUsers()
{
    var repository = new InMemoryUserRepository(
    [
        User.Rehydrate(1, "active-admin", "hash", null, null, UserRole.Admin, DateTime.UtcNow, false, null),
    ]);

    var service = new RestoreUserService(repository, new TestLogger<RestoreUserService>());
    var activeResult = await service.ExecuteAsync(new RestoreUserCommand(1));
    var missingResult = await service.ExecuteAsync(new RestoreUserCommand(999));

    Assert(!activeResult.Success, "Expected active user restore to fail.");
    Assert(!missingResult.Success, "Expected missing user restore to fail.");
}

static async Task RestoreUserRestoresDeletedUser()
{
    var repository = new InMemoryUserRepository(
    [
        User.Rehydrate(1, "deleted-admin", "hash", null, null, UserRole.Admin, DateTime.UtcNow, true, DateTime.UtcNow),
    ]);

    var service = new RestoreUserService(repository, new TestLogger<RestoreUserService>());
    var result = await service.ExecuteAsync(new RestoreUserCommand(1));

    Assert(result.Success, "Expected deleted user restore to succeed.");
    Assert((await repository.ListDeletedAsync()).All(user => user.UserId != 1), "Expected user to be removed from deleted list.");
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

internal sealed class InMemoryUserRepository(List<User> users) : IUserRepository
{
    private readonly List<User> _users = users;

    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(user => user.UserId == userId && !user.IsDeleted));
    }

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(user =>
            !user.IsDeleted && string.Equals(user.UserName, userName.Trim(), StringComparison.OrdinalIgnoreCase)));
    }

    public Task<bool> UserNameExistsAsync(string userName, int? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var exists = _users.Any(user =>
            !user.IsDeleted &&
            string.Equals(user.UserName, userName.Trim(), StringComparison.OrdinalIgnoreCase) &&
            (!excludeUserId.HasValue || user.UserId != excludeUserId.Value));

        return Task.FromResult(exists);
    }

    public Task<IEnumerable<User>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.Where(user => !user.IsDeleted));
    }

    public Task<IEnumerable<User>> ListDeletedAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.Where(user => user.IsDeleted));
    }

    public Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var index = _users.FindIndex(existing => existing.UserId == user.UserId);
        if (index >= 0)
        {
            _users[index] = user;
        }

        return Task.FromResult(user);
    }

    public Task<bool> SoftDeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(existing => existing.UserId == userId && !existing.IsDeleted);
        user?.MarkDeleted();
        return Task.FromResult(user is not null);
    }

    public Task<bool> RestoreDeletedAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(existing => existing.UserId == userId && existing.IsDeleted);
        if (user is null)
        {
            return Task.FromResult(false);
        }

        var restored = User.Rehydrate(
            user.UserId,
            user.UserName,
            user.PasswordHash,
            user.PhoneNumber,
            user.Address,
            user.Role,
            user.CreatedAt,
            false,
            null);

        _users[_users.IndexOf(user)] = restored;
        return Task.FromResult(true);
    }

    public Task<bool> HardDeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.RemoveAll(user => user.UserId == userId) > 0);
    }
}

internal sealed class TestLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => false;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
    }
}
