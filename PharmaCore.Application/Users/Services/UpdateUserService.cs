using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Auth;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Users.Dtos;
using PharmaCore.Application.Users.Interfaces;
using PharmaCore.Application.Users.Requests;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Users.Services;

public class UpdateUserService : IUpdateUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UpdateUserService> _logger;

    public UpdateUserService(IUserRepository userRepository, IPasswordHasher passwordHasher, ILogger<UpdateUserService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<ServiceResult<UpdatedUserDto>> ExecuteAsync(UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null || user.IsDeleted)
        {
            _logger.LogWarning("Failed to update user {UserId}: user not found", command.UserId);
            return ServiceResult<UpdatedUserDto>.Fail(ServiceErrorType.NotFound, "User not found");
        }

        if (command.UserName is not null && await _userRepository.UserNameExistsAsync(command.UserName, command.UserId, cancellationToken))
        {
            _logger.LogWarning("Failed to update user {UserId}: username '{UserName}' already exists", command.UserId, command.UserName);
            return ServiceResult<UpdatedUserDto>.Fail(ServiceErrorType.Duplicate, "Username already exists");
        }

        UserRole? role = null;
        if (command.Role.HasValue)
        {
            if (!Enum.IsDefined(typeof(UserRole), command.Role.Value))
            {
                _logger.LogWarning("Failed to update user {UserId}: invalid role {Role}", command.UserId, command.Role.Value);
                return ServiceResult<UpdatedUserDto>.Fail(ServiceErrorType.Validation, "Invalid role.");
            }

            role = (UserRole)command.Role.Value;
        }

        user.UpdateProfile(command.UserName, command.PhoneNumber, command.Address, role);

        if (command.Password is not null)
        {
            if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Trim().Length < 6)
            {
                return ServiceResult<UpdatedUserDto>.Fail(ServiceErrorType.Validation, "Password must be at least 6 characters long.");
            }

            user.ChangePassword(_passwordHasher.Hash(command.Password));
        }

        var updated = await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User '{UserName}' (ID: {UserId}) updated successfully", updated.UserName, updated.UserId);

        return ServiceResult<UpdatedUserDto>.Ok(
            new UpdatedUserDto(
                updated.UserId,
                updated.UserName,
                updated.PhoneNumber,
                updated.Address,
                (short)updated.Role,
                DateTime.UtcNow));
    }
}
