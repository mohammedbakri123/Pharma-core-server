using PharmaCore.Application.Users.Dtos;
using PharmaCore.Application.Users.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Users.Interfaces;

public interface ICreateUserService
{
    Task<ServiceResult<UserDto>> ExecuteAsync(CreateUserCommand command, CancellationToken cancellationToken = default);
}
