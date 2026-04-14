using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.Categories;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Requests;
using PharmaCore.Application.Categories.Dtos;
using PharmaCore.Application.Common.Exceptions;

namespace PharmaCore.API.Controllers;

/// <summary>
/// Manages product categories.
/// </summary>
[Route("categories")]
[Authorize]
[Tags("Categories")]
public class CategoriesController : ApiControllerBase
{
    /// <summary>
    /// Returns a paginated list of categories, optionally filtered by search term.
    /// </summary>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="limit">Items per page (default 20).</param>
    /// <param name="search">Optional search keyword to filter by name.</param>
    /// <param name="listCategoriesService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Paginated list of categories.</response>
    /// <response code="400">Validation error (e.g. page &lt; 1).</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromServices] IListCategoriesService listCategoriesService = null!,
        CancellationToken cancellationToken = default)
    {
        page = page <= 0 ? 1 : page;
        limit = limit <= 0 ? 20 : limit;

        return await MapAppExceptionAsync(async () =>
        {
            var result = await listCategoriesService.ExecuteAsync(
                new ListCategoriesQuery(page, limit, search), cancellationToken);

            return Ok(new
            {
                categories = result.Items,
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
    /// Returns a single category by its ID.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="getCategoryByIdService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The requested category.</response>
    /// <response code="404">Category not found.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] IGetCategoryByIdService getCategoryByIdService,
        CancellationToken cancellationToken)
    {
        return await MapAppExceptionAsync(async () =>
        {
            var category = await getCategoryByIdService.ExecuteAsync(
                new GetCategoryByIdQuery(id), cancellationToken);

            if (category == null)
            {
                return NotFound(new { error = "Category not found" });
            }

            return Ok(category);
        });
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="request">The category data to create.</param>
    /// <param name="createCategoryService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">The created category.</response>
    /// <response code="400">Validation error (e.g. missing name).</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    /// <response code="409">A category with the same name already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request,
        [FromServices] ICreateCategoryService createCategoryService,
        CancellationToken cancellationToken)
    {
        return await MapAppExceptionAsync(async () =>
        {
            var category = await createCategoryService.ExecuteAsync(
                new CreateCategoryCommand(request.CategoryName, request.CategoryArabicName),
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, category);
        });
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="request">The updated category data.</param>
    /// <param name="updateCategoryService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The updated category.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Category not found.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCategoryRequest request,
        [FromServices] IUpdateCategoryService updateCategoryService,
        CancellationToken cancellationToken)
    {
        return await MapAppExceptionAsync(async () =>
        {
            var category = await updateCategoryService.ExecuteAsync(
                new UpdateCategoryCommand(id, request.CategoryName, request.CategoryArabicName),
                cancellationToken);

            return Ok(category);
        });
    }

    /// <summary>
    /// Soft-deletes a category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="deleteCategoryService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Confirmation message.</response>
    /// <response code="404">Category not found.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] IDeleteCategoryService deleteCategoryService,
        CancellationToken cancellationToken)
    {
        return await MapAppExceptionAsync(async () =>
        {
            await deleteCategoryService.ExecuteAsync(id, cancellationToken);
            return Ok(new { message = "Category deleted successfully" });
        });
    }
}
