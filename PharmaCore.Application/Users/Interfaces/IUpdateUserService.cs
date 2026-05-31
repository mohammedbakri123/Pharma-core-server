using PharmaCore.Application.Users.Dtos;
using PharmaCore.Application.Users.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Users.Interfaces;

public interface IUpdateUserService
{
    Task<ServiceResult<UpdatedUserDto>> ExecuteAsync(UpdateUserCommand command, CancellationToken cancellationToken = default);
}
