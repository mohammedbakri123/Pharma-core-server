using PharmaCore.Application.Auth.Dtos;

namespace PharmaCore.Application.Auth.Interfaces;

public interface IGetCurrentUserService
{
    Task<CurrentUserDto> ExecuteAsync(int userId, CancellationToken cancellationToken = default);
}
