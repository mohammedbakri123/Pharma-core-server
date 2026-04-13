using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Exceptions;
using PharmaCore.Application.Users.Interfaces;

namespace PharmaCore.Application.Users.Services;

public class DeleteUserService : IDeleteUserService
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
