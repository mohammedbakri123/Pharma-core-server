using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Users.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Users.Services;

public class HardDeleteUserService : IHardDeleteUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<HardDeleteUserService> _logger;

    public HardDeleteUserService(IUserRepository userRepository, ILogger<HardDeleteUserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<bool>> ExecuteAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"User with ID {userId} not found.");
            }

            var result = await _userRepository.HardDeleteAsync(userId, cancellationToken);

            if (!result)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, "Failed to permanently delete user.");
            }

            _logger.LogInformation("User with ID {Id} permanently deleted", userId);

            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error permanently deleting user {UserId}", userId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error permanently deleting user: {e.Message}");
        }
    }
}