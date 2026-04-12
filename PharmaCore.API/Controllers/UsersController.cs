using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.Users;
using PharmaCore.Application.Users.Requests;
using PharmaCore.Application.Users.Services;

namespace PharmaCore.API.Controllers;

[Route("users")]
[Authorize]
public class UsersController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page,
        [FromQuery] int limit,
        [FromQuery] short? role,
        [FromQuery] string? search,
        [FromServices] ListUsersService listUsersService,
        CancellationToken cancellationToken)
    {
        page = page <= 0 ? 1 : page;
        limit = limit <= 0 ? 20 : limit;

        return await MapAppExceptionAsync(async () =>
        {
            var result = await listUsersService.ExecuteAsync(
                new ListUsersQuery(page, limit, role, search), cancellationToken);

            return Ok(new
            {
                users = result.Items,
                pagination = new
                {
                    total = result.Total,
                    page = result.Page,
                    limit = result.Limit
                }
            });
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        [FromServices] CreateUserService createUserService,
        CancellationToken cancellationToken)
    {
        return await MapAppExceptionAsync(async () =>
        {
            var user = await createUserService.ExecuteAsync(
                new CreateUserCommand(request.UserName, request.Password, request.PhoneNumber, request.Address, request.Role),
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, user);
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateUserRequest request,
        [FromServices] UpdateUserService updateUserService,
        CancellationToken cancellationToken)
    {
        return await MapAppExceptionAsync(async () =>
        {
            var user = await updateUserService.ExecuteAsync(
                new UpdateUserCommand(id, request.UserName, request.Password, request.PhoneNumber, request.Address, request.Role),
                cancellationToken);

            return Ok(user);
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] DeleteUserService deleteUserService,
        CancellationToken cancellationToken)
    {
        return await MapAppExceptionAsync(async () =>
        {
            await deleteUserService.ExecuteAsync(id, cancellationToken);
            return Ok(new { message = "User deleted successfully" });
        });
    }
}
