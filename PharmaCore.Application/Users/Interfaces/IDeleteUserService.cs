using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Users.Interfaces;

public interface IDeleteUserService
{
    Task<ServiceResult<bool>> ExecuteAsync(int userId, CancellationToken cancellationToken = default);
}
