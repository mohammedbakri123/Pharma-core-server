using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.Categories;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Requests;

namespace PharmaCore.API.Controllers;

[Route("categories")]
[Authorize]
public class CategoriesController : ApiControllerBase
{
    [HttpGet]
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

    [HttpGet("{id:int}")]
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

    [HttpPost]
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

    [HttpPut("{id:int}")]
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

    [HttpDelete("{id:int}")]
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
