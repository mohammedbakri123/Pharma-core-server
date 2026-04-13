using PharmaCore.Application.Users.Dtos;
using PharmaCore.Application.Users.Requests;

namespace PharmaCore.Application.Users.Interfaces;

public interface ICreateUserService
{
    Task<UserDto> ExecuteAsync(CreateUserCommand command, CancellationToken cancellationToken = default);
}
