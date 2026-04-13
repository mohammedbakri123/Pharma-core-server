using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Exceptions;
using PharmaCore.Application.Users.Interfaces;

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

    public async Task ExecuteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var deleted = await _userRepository.SoftDeleteAsync(userId, cancellationToken);
        if (!deleted)
        {
            _logger.LogWarning("Failed to delete user {UserId}: user not found", userId);
            throw new NotFoundException("User not found");
        }

        _logger.LogInformation("User {UserId} deleted successfully", userId);
    }
}
