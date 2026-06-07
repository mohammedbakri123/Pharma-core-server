using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Users.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Users.Services;

public class DeleteUserService(IUserRepository userRepository, ILogger<DeleteUserService> logger)
    : IDeleteUserService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await userRepository.SoftDeleteAsync(userId, cancellationToken);
            if (!deleted)
            {
                logger.LogWarning("Failed to delete user {UserId}: user not found", userId);
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "User not found");
            }

            logger.LogInformation("User {UserId} deleted successfully", userId);

            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting user {UserId}", userId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error deleting user: {e.Message}");
        }
    }
}
