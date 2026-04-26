using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Users.Dtos;
using PharmaCore.Application.Users.Interfaces;
using PharmaCore.Application.Users.Requests;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Users.Services;

public class ListDeletedUsersService(IUserRepository userRepository, ILogger<ListDeletedUsersService> logger)
    : IListDeletedUsersService
{
    public async Task<ServiceResult<PagedResult<UserDto>>> ExecuteAsync(ListDeletedUsersQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            if (query.Page <= 0 || query.Limit <= 0)
            {
                return ServiceResult<PagedResult<UserDto>>.Fail(ServiceErrorType.Validation, "Page and limit must be greater than zero.");
            }

            UserRole? role = null;
            if (query.Role.HasValue)
            {
                if (!Enum.IsDefined(typeof(UserRole), query.Role.Value))
                {
                    return ServiceResult<PagedResult<UserDto>>.Fail(ServiceErrorType.Validation, "Invalid role.");
                }

                role = (UserRole)query.Role.Value;
            }

            var users = await userRepository.ListDeletedAsync(cancellationToken);
            var filtered = users.AsQueryable();

            if (role.HasValue)
            {
                filtered = filtered.Where(user => user.Role == role.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLowerInvariant();
                filtered = filtered.Where(user =>
                    user.UserName.ToLowerInvariant().Contains(search) ||
                    (user.PhoneNumber != null && user.PhoneNumber.ToLowerInvariant().Contains(search)));
            }

            var total = filtered.Count();
            var items = filtered
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .Select(user => new UserDto(user.UserId, user.UserName, user.PhoneNumber, user.Address, (short)user.Role, user.CreatedAt))
                .ToList();

            return ServiceResult<PagedResult<UserDto>>.Ok(new PagedResult<UserDto>(items, total, query.Page, query.Limit));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error listing deleted users");
            return ServiceResult<PagedResult<UserDto>>.Fail(ServiceErrorType.ServerError, $"Error listing deleted users: {e.Message}");
        }
    }
}
