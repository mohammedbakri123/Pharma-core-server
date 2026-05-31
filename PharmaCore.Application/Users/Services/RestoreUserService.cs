using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Users.Interfaces;
using PharmaCore.Application.Users.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Users.Services;

public class RestoreUserService(IUserRepository userRepository, ILogger<RestoreUserService> logger)
    : IRestoreUserService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(RestoreUserCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var restored = await userRepository.RestoreDeletedAsync(command.UserId, cancellationToken);

            if (!restored)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Deleted user with ID {command.UserId} not found or is not deleted.");
            }

            logger.LogInformation("User with ID {UserId} restored successfully", command.UserId);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error restoring user {UserId}", command.UserId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error restoring user: {e.Message}");
        }
    }
}
