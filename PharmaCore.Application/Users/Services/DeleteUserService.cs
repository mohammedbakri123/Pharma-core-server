using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Exceptions;

namespace PharmaCore.Application.Users.Services;

public class DeleteUserService
{
    private readonly IUserRepository _userRepository;

    public DeleteUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task ExecuteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var deleted = await _userRepository.SoftDeleteAsync(userId, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException("User not found");
        }
    }
}
