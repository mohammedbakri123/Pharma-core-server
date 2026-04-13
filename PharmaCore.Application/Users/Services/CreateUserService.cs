using PharmaCore.Application.Abstractions.Auth;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Exceptions;
using PharmaCore.Application.Users.Dtos;
using PharmaCore.Application.Users.Interfaces;
using PharmaCore.Application.Users.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Users.Services;

public class CreateUserService : ICreateUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> ExecuteAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        ValidatePassword(command.Password);

        if (!Enum.IsDefined(typeof(UserRole), command.Role))
        {
            throw new AppValidationException("Invalid role.");
        }

        if (await _userRepository.UserNameExistsAsync(command.UserName, null, cancellationToken))
        {
            throw new ConflictException("Username already exists");
        }

        var user = User.Create(
            command.UserName,
            _passwordHasher.Hash(command.Password),
            command.PhoneNumber,
            command.Address,
            (UserRole)command.Role);

        var created = await _userRepository.AddAsync(user, cancellationToken);
        return new UserDto(created.UserId, created.UserName, created.PhoneNumber, created.Address, (short)created.Role, created.CreatedAt);
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Trim().Length < 6)
        {
            throw new AppValidationException("Password must be at least 6 characters long.");
        }
    }
}
