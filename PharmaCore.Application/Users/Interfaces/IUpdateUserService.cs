using PharmaCore.Application.Users.Dtos;
using PharmaCore.Application.Users.Requests;

namespace PharmaCore.Application.Users.Interfaces;

public interface IUpdateUserService
{
    Task<UpdatedUserDto> ExecuteAsync(UpdateUserCommand command, CancellationToken cancellationToken = default);
}
