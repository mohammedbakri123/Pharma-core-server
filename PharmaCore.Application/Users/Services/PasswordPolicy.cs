namespace PharmaCore.Application.Users.Services;

internal static class PasswordPolicy
{
    public static string? Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            return "Password must be at least 8 characters long.";
        }

        if (!password.Any(char.IsUpper))
        {
            return "Password must contain at least one uppercase letter.";
        }

        if (!password.Any(char.IsLower))
        {
            return "Password must contain at least one lowercase letter.";
        }

        if (!password.Any(char.IsDigit))
        {
            return "Password must contain at least one number.";
        }

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
        {
            return "Password must contain at least one special character.";
        }

        return null;
    }
}
