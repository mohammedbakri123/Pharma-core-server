using PharmaCore.Application.Auth.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Auth.Interfaces;

public interface IGetCurrentUserService
{
    Task<ServiceResult<CurrentUserDto>> ExecuteAsync(int userId, CancellationToken cancellationToken = default);
}
