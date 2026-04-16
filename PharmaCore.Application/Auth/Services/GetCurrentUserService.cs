using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Auth.Dtos;
using PharmaCore.Application.Auth.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Auth.Services;

public class GetCurrentUserService : IGetCurrentUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetCurrentUserService> _logger;

    public GetCurrentUserService(IUserRepository userRepository, ILogger<GetCurrentUserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<CurrentUserDto>> ExecuteAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user is null || user.IsDeleted)
            {
                _logger.LogWarning("GetCurrentUser failed: user {UserId} not found or deleted", userId);
                return ServiceResult<CurrentUserDto>.Fail(ServiceErrorType.Unauthorized, "Unauthorized");
            }

            return ServiceResult<CurrentUserDto>.Ok(
                new CurrentUserDto(
                    user.UserId,
                    user.UserName,
                    user.PhoneNumber,
                    user.Address,
                    (short)user.Role,
                    user.CreatedAt));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting current user {UserId}", userId);
            return ServiceResult<CurrentUserDto>.Fail(ServiceErrorType.ServerError, $"Error getting current user: {e.Message}");
        }
    }
}
