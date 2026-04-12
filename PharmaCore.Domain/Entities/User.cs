using PharmaCore.Domain.Enums;

namespace PharmaCore.Domain.Entities;

public sealed class User
{
    private User(
        int userId,
        string userName,
        string passwordHash,
        string? phoneNumber,
        string? address,
        UserRole role,
        DateTime? createdAt,
        bool isDeleted,
        DateTime? deletedAt)
    {
        UserId = userId;
        UserName = ValidateUserName(userName);
        PasswordHash = ValidatePasswordHash(passwordHash);
        PhoneNumber = NormalizeOptional(phoneNumber);
        Address = NormalizeOptional(address);
        Role = ValidateRole(role);
        CreatedAt = createdAt;
        IsDeleted = isDeleted;
        DeletedAt = deletedAt;
    }

    public int UserId { get; private set; }
    public string UserName { get; private set; }
    public string PasswordHash { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Address { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public static User Create(
        string userName,
        string passwordHash,
        string? phoneNumber,
        string? address,
        UserRole role)
    {
        return new User(0, userName, passwordHash, phoneNumber, address, role, null, false, null);
    }

    public static User Rehydrate(
        int userId,
        string userName,
        string passwordHash,
        string? phoneNumber,
        string? address,
        UserRole role,
        DateTime? createdAt,
        bool isDeleted,
        DateTime? deletedAt)
    {
        return new User(userId, userName, passwordHash, phoneNumber, address, role, createdAt, isDeleted, deletedAt);
    }

    public void UpdateProfile(string? userName, string? phoneNumber, string? address, UserRole? role)
    {
        if (userName is not null)
        {
            UserName = ValidateUserName(userName);
        }

        if (phoneNumber is not null)
        {
            PhoneNumber = NormalizeOptional(phoneNumber);
        }

        if (address is not null)
        {
            Address = NormalizeOptional(address);
        }

        if (role.HasValue)
        {
            Role = ValidateRole(role.Value);
        }
    }

    public void ChangePassword(string passwordHash)
    {
        PasswordHash = ValidatePasswordHash(passwordHash);
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void SetPersistedState(int userId, DateTime? createdAt)
    {
        UserId = userId;
        CreatedAt = createdAt;
    }

    private static string ValidateUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("User name is required.", nameof(userName));
        }

        var normalized = userName.Trim();
        if (normalized.Length < 3)
        {
            throw new ArgumentException("User name must be at least 3 characters long.", nameof(userName));
        }

        return normalized;
    }

    private static string ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        return passwordHash;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static UserRole ValidateRole(UserRole role)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role), "Invalid user role.");
        }

        return role;
    }
}
