using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.Users;
using PharmaCore.Application.Users.Interfaces;
using PharmaCore.Application.Users.Requests;
using PharmaCore.Application.Users.Dtos;

namespace PharmaCore.API.Controllers;

/// <summary>
/// Manages system users.
/// </summary>
[Route("users")]
[Authorize]
[Tags("Users")]
public class UsersController : ApiControllerBase
{
    /// <summary>
    /// Returns a paginated list of users, optionally filtered by role or search term.
    /// </summary>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="limit">Items per page (default 20).</param>
    /// <param name="role">Optional role filter (see UserRole enum).</param>
    /// <param name="search">Optional search keyword.</param>
    /// <param name="listUsersService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Paginated list of users.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] int page,
        [FromQuery] int limit,
        [FromQuery] short? role,
        [FromQuery] string? search,
        [FromServices] IListUsersService listUsersService,
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

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="request">The user data to create.</param>
    /// <param name="createUserService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">The created user.</response>
    /// <response code="400">Validation error (e.g. weak password).</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    /// <response code="409">Username already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        [FromServices] ICreateUserService createUserService,
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

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="request">The updated user data.</param>
    /// <param name="updateUserService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The updated user.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">User not found.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    /// <response code="409">Username already exists.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateUserRequest request,
        [FromServices] IUpdateUserService updateUserService,
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

    /// <summary>
    /// Soft-deletes a user.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="deleteUserService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Confirmation message.</response>
    /// <response code="404">User not found.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] IDeleteUserService deleteUserService,
        CancellationToken cancellationToken)
    {
        return await MapAppExceptionAsync(async () =>
        {
            await deleteUserService.ExecuteAsync(id, cancellationToken);
            return Ok(new { message = "User deleted successfully" });
        });
    }
}
