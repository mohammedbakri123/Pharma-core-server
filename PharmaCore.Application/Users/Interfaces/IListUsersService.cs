using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Users.Dtos;
using PharmaCore.Application.Users.Requests;

namespace PharmaCore.Application.Users.Interfaces;

public interface IListUsersService
{
    Task<PagedResult<UserDto>> ExecuteAsync(ListUsersQuery query, CancellationToken cancellationToken = default);
}
