using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Exceptions;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Users.Dtos;
using PharmaCore.Application.Users.Interfaces;
using PharmaCore.Application.Users.Requests;
using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Users.Services;

public class ListUsersService : IListUsersService
{
    private readonly IUserRepository _userRepository;

    public ListUsersService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<UserDto>> ExecuteAsync(ListUsersQuery query, CancellationToken cancellationToken = default)
    {
        if (query.Page <= 0 || query.Limit <= 0)
        {
            throw new AppValidationException("Page and limit must be greater than zero.");
        }

        UserRole? role = null;
        if (query.Role.HasValue)
        {
            if (!Enum.IsDefined(typeof(UserRole), query.Role.Value))
            {
                throw new AppValidationException("Invalid role.");
            }

            role = (UserRole)query.Role.Value;
        }

        var result = await _userRepository.ListAsync(query.Page, query.Limit, role, query.Search, cancellationToken);

        return new PagedResult<UserDto>(
            result.Items.Select(Map).ToList(),
            result.Total,
            result.Page,
            result.Limit);
    }

    private static UserDto Map(Domain.Entities.User user)
    {
        return new UserDto(user.UserId, user.UserName, user.PhoneNumber, user.Address, (short)user.Role, user.CreatedAt);
    }
}
