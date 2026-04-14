using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Users.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Users.Services;

public class DeleteUserService : IDeleteUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DeleteUserService> _logger;

    public DeleteUserService(IUserRepository userRepository, ILogger<DeleteUserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<bool>> ExecuteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var deleted = await _userRepository.SoftDeleteAsync(userId, cancellationToken);
        if (!deleted)
        {
            _logger.LogWarning("Failed to delete user {UserId}: user not found", userId);
            return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "User not found");
        }

        _logger.LogInformation("User {UserId} deleted successfully", userId);

        return ServiceResult<bool>.Ok(true);
    }
}
