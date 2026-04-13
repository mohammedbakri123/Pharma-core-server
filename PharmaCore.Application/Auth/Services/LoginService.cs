using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Auth;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Auth.Interfaces;
using PharmaCore.Application.Auth.Dtos;
using PharmaCore.Application.Auth.Requests;
using PharmaCore.Application.Common.Exceptions;

namespace PharmaCore.Application.Auth.Services;

public class LoginService : ILoginService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginService> _logger;

    public LoginService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService, ILogger<LoginService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponseDto> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.UserName) || string.IsNullOrWhiteSpace(command.Password))
        {
            _logger.LogWarning("Login attempt with empty credentials");
            throw new AppValidationException("User name and password are required.");
        }

        var user = await _userRepository.GetByUserNameAsync(command.UserName.Trim(), cancellationToken);

        if (user is null || user.IsDeleted || !_passwordHasher.Verify(user.PasswordHash, command.Password))
        {
            _logger.LogWarning("Login failed for user '{UserName}': invalid credentials", command.UserName);
            throw new UnauthorizedException("Invalid credentials");
        }

        _logger.LogInformation("User '{UserName}' logged in successfully", user.UserName);

        return new LoginResponseDto(
            _tokenService.CreateToken(user),
            new AuthenticatedUserDto(user.UserId, user.UserName, (short)user.Role));
    }
}
