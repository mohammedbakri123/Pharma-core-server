using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Auth.Dtos;
using PharmaCore.Application.Auth.Interfaces;
using PharmaCore.Application.Common.Exceptions;

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

    public async Task<CurrentUserDto> ExecuteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null || user.IsDeleted)
        {
            _logger.LogWarning("GetCurrentUser failed: user {UserId} not found or deleted", userId);
            throw new UnauthorizedException("Unauthorized");
        }

        return new CurrentUserDto(
            user.UserId,
            user.UserName,
            user.PhoneNumber,
            user.Address,
            (short)user.Role,
            user.CreatedAt);
    }
}
