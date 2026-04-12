using PharmaCore.Application.Abstractions.Auth;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Exceptions;
using PharmaCore.Application.Users.Dtos;
using PharmaCore.Application.Users.Requests;
using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Users.Services;

public class UpdateUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UpdateUserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UpdatedUserDto> ExecuteAsync(UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null || user.IsDeleted)
        {
            throw new NotFoundException("User not found");
        }

        if (command.UserName is not null && await _userRepository.UserNameExistsAsync(command.UserName, command.UserId, cancellationToken))
        {
            throw new ConflictException("Username already exists");
        }

        UserRole? role = null;
        if (command.Role.HasValue)
        {
            if (!Enum.IsDefined(typeof(UserRole), command.Role.Value))
            {
                throw new AppValidationException("Invalid role.");
            }

            role = (UserRole)command.Role.Value;
        }

        user.UpdateProfile(command.UserName, command.PhoneNumber, command.Address, role);

        if (command.Password is not null)
        {
            if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Trim().Length < 6)
            {
                throw new AppValidationException("Password must be at least 6 characters long.");
            }

            user.ChangePassword(_passwordHasher.Hash(command.Password));
        }

        var updated = await _userRepository.UpdateAsync(user, cancellationToken);

        return new UpdatedUserDto(
            updated.UserId,
            updated.UserName,
            updated.PhoneNumber,
            updated.Address,
            (short)updated.Role,
            DateTime.UtcNow);
    }
}
