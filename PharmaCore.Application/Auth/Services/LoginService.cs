using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Auth;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Auth.Interfaces;
using PharmaCore.Application.Auth.Dtos;
using PharmaCore.Application.Auth.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Auth.Services;

public class LoginService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ILogger<LoginService> logger)
    : ILoginService
{
    public async Task<ServiceResult<LoginResponseDto>> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.UserName) || string.IsNullOrWhiteSpace(command.Password))
            {
                logger.LogWarning("Login attempt with empty credentials");
                return ServiceResult<LoginResponseDto>.Fail(ServiceErrorType.Validation, "User name and password are required.");
            }

            var user = await userRepository.GetByUserNameAsync(command.UserName.Trim(), cancellationToken);

            if (user is null || user.IsDeleted || !passwordHasher.Verify(user.PasswordHash, command.Password))
            {
                logger.LogWarning("Login failed for user '{UserName}': invalid credentials", command.UserName);
                return ServiceResult<LoginResponseDto>.Fail(ServiceErrorType.Unauthorized, "Invalid credentials");
            }

            logger.LogInformation("User '{UserName}' logged in successfully", user.UserName);

            return ServiceResult<LoginResponseDto>.Ok(
                new LoginResponseDto(
                    tokenService.CreateToken(user),
                    new AuthenticatedUserDto(user.UserId, user.UserName, (short)user.Role)));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error logging in user '{UserName}'", command.UserName);
            return ServiceResult<LoginResponseDto>.Fail(ServiceErrorType.ServerError, $"Error logging in: {e.Message}");
        }
    }
}
