using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Auth.Dtos;
using PharmaCore.Application.Common.Exceptions;

namespace PharmaCore.Application.Auth.Services;

public class GetCurrentUserService
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<CurrentUserDto> ExecuteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null || user.IsDeleted)
        {
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
