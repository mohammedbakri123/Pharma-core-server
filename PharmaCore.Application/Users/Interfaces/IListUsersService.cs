using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Users.Dtos;
using PharmaCore.Application.Users.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Users.Interfaces;

public interface IListUsersService
{
    Task<ServiceResult<PagedResult<UserDto>>> ExecuteAsync(ListUsersQuery query, CancellationToken cancellationToken = default);
}
