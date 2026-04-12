using PharmaCore.Application.Abstractions.Auth;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Auth.Dtos;
using PharmaCore.Application.Auth.Requests;
using PharmaCore.Application.Common.Exceptions;

namespace PharmaCore.Application.Auth.Services;

public class LoginService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.UserName) || string.IsNullOrWhiteSpace(command.Password))
        {
            throw new AppValidationException("User name and password are required.");
        }

        var user = await _userRepository.GetByUserNameAsync(command.UserName.Trim(), cancellationToken);

        if (user is null || user.IsDeleted || !_passwordHasher.Verify(user.PasswordHash, command.Password))
        {
            throw new UnauthorizedException("Invalid credentials");
        }

        return new LoginResponseDto(
            _tokenService.CreateToken(user),
            new AuthenticatedUserDto(user.UserId, user.UserName, (short)user.Role));
    }
}
