using PharmaCore.Application.Users.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Users.Interfaces;

public interface IRestoreUserService
{
    Task<ServiceResult<bool>> ExecuteAsync(RestoreUserCommand command, CancellationToken cancellationToken = default);
}
