using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Auth;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Users.Dtos;
using PharmaCore.Application.Users.Interfaces;
using PharmaCore.Application.Users.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Users.Services;

public class CreateUserService : ICreateUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<CreateUserService> _logger;

    public CreateUserService(IUserRepository userRepository, IPasswordHasher passwordHasher, ILogger<CreateUserService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<ServiceResult<UserDto>> ExecuteAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        var passwordValidation = ValidatePassword(command.Password);
        if (!string.IsNullOrEmpty(passwordValidation))
        {
            return ServiceResult<UserDto>.Fail(ServiceErrorType.Validation, passwordValidation);
        }

        if (!Enum.IsDefined(typeof(UserRole), command.Role))
        {
            _logger.LogWarning("Failed to create user '{UserName}': invalid role {Role}", command.UserName, command.Role);
            return ServiceResult<UserDto>.Fail(ServiceErrorType.Validation, "Invalid role.");
        }

        if (await _userRepository.UserNameExistsAsync(command.UserName, null, cancellationToken))
        {
            _logger.LogWarning("Failed to create user: username '{UserName}' already exists", command.UserName);
            return ServiceResult<UserDto>.Fail(ServiceErrorType.Duplicate, "Username already exists");
        }

        var user = User.Create(
            command.UserName,
            _passwordHasher.Hash(command.Password),
            command.PhoneNumber,
            command.Address,
            (UserRole)command.Role);

        var created = await _userRepository.AddAsync(user, cancellationToken);

        _logger.LogInformation("User '{UserName}' created successfully with ID {UserId}", created.UserName, created.UserId);

        return ServiceResult<UserDto>.Ok(
            new UserDto(created.UserId, created.UserName, created.PhoneNumber, created.Address, (short)created.Role, created.CreatedAt));
    }

    private static string? ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Trim().Length < 6)
        {
            return "Password must be at least 6 characters long.";
        }
        return null;
    }
}
