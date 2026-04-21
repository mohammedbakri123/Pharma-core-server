using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Application.Medicine.Interfaces;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.API.Contracts.Medicine;
using PharmaCore.Domain.Enums;

namespace PharmaCore.API.Controllers;

/// <summary>
/// Manages medicines in the pharmacy inventory.
/// </summary>
[Route("medicines")]
[Authorize]
[Tags("Medicines")]
public class MedicinesController : ApiControllerBase
{
    /// <summary>
    /// Returns a paginated list of medicines, optionally filtered by search term, unit, or category.
    /// </summary>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="limit">Items per page (default 20).</param>
    /// <param name="search">Optional search keyword to filter by name, Arabic name, or barcode.</param>
    /// <param name="unit">Optional unit filter (e.g. box, strip, pill).</param>
    /// <param name="categoryId">Optional category ID filter.</param>
    /// <param name="listMedicineService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Paginated list of medicines.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] MedicineUnit? unit = null,
        [FromQuery] int? categoryId = null,
        [FromServices] IListMedicineService listMedicineService = null!,
        CancellationToken cancellationToken = default)
    {
        page = page <= 0 ? 1 : page;
        limit = limit <= 0 ? 20 : limit;

        var result = await listMedicineService.ExecuteAsync(
            new ListMedicineQuery(page, limit, search, unit, categoryId), cancellationToken);

        if (!result.Success)
        {
            return MapServiceResult(result);
        }

        return Ok(new
        {
            medicines = result.Data!.Items,
            pagination = new
            {
                total = result.Data.Total,
                page = result.Data.Page,
                limit = result.Data.Limit
            }
        });
    }

    /// <summary>
    /// Returns a single medicine by its ID.
    /// </summary>
    /// <param name="id">The medicine ID.</param>
    /// <param name="getMedicineByIdService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The requested medicine.</response>
    /// <response code="404">Medicine not found.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MedicineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] IGetMedicineByIdService getMedicineByIdService,
        CancellationToken cancellationToken)
    {
        var result = await getMedicineByIdService.ExecuteAsync(
            new GetMedicineByIdQuery(id), cancellationToken);

        return MapServiceResult(result);
    }

    /// <summary>
    /// Creates a new medicine.
    /// </summary>
    /// <param name="request">The medicine data to create.</param>
    /// <param name="createMedicineService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">The created medicine.</response>
    /// <response code="400">Validation error (e.g. missing name).</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpPost]
    [ProducesResponseType(typeof(MedicineDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMedicineRequest request,
        [FromServices] ICreateMedicineService createMedicineService,
        CancellationToken cancellationToken)
    {
        var unitEnum = request.Unit.HasValue ? (MedicineUnit)request.Unit.Value : (MedicineUnit?)null;

        var result = await createMedicineService.ExecuteAsync(
            new CreateMedicineCommand(
                request.Name,
                request.ArabicName,
                request.Barcode,
                request.CategoryId,
                unitEnum),
            cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return StatusCode(201, result.Data);
    }

    /// <summary>
    /// Returns a paginated list of soft-deleted medicines.
    /// </summary>
    [HttpGet("deleted")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListDeleted(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] MedicineUnit? unit = null,
        [FromQuery] int? categoryId = null,
        [FromServices] IListDeletedMedicinesService listDeletedMedicinesService = null!,
        CancellationToken cancellationToken = default)
    {
        page = page <= 0 ? 1 : page;
        limit = limit <= 0 ? 20 : limit;

        var result = await listDeletedMedicinesService.ExecuteAsync(
            new ListDeletedMedicinesQuery(page, limit, search, unit, categoryId), cancellationToken);

        if (!result.Success)
        {
            return MapServiceResult(result);
        }

        return Ok(new
        {
            medicines = result.Data!.Items,
            pagination = new
            {
                total = result.Data.Total,
                page = result.Data.Page,
                limit = result.Data.Limit
            }
        });
    }

    /// <summary>
    /// Restores a soft-deleted medicine.
    /// </summary>
    [HttpPost("{id:int}/restore")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restore(
        int id,
        [FromServices] IRestoreMedicineService restoreMedicineService,
        CancellationToken cancellationToken)
    {
        var result = await restoreMedicineService.ExecuteAsync(
            new RestoreMedicineCommand(id), cancellationToken);

        if (!result.Success)
        {
            return MapServiceResult(result);
        }

        return Ok(new { message = "Medicine restored successfully" });
    }

    /// <summary>
    /// Updates an existing medicine.
    /// </summary>
    /// <param name="id">The medicine ID.</param>
    /// <param name="request">The updated medicine data.</param>
    /// <param name="updateMedicineService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The updated medicine.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Medicine not found.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MedicineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateMedicineRequest request,
        [FromServices] IUpdateMedicineService updateMedicineService,
        CancellationToken cancellationToken)
    {
        var unitEnum = request.Unit.HasValue ? (MedicineUnit)request.Unit.Value : (MedicineUnit?)null;

        var result = await updateMedicineService.ExecuteAsync(
            new UpdateMedicineCommand(
                id,
                request.Name,
                request.ArabicName,
                request.Barcode,
                request.CategoryId,
                unitEnum),
            cancellationToken);

        return MapServiceResult(result);
    }

    /// <summary>
    /// Soft-deletes a medicine.
    /// </summary>
    /// <param name="id">The medicine ID.</param>
    /// <param name="deleteMedicineService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Confirmation message.</response>
    /// <response code="404">Medicine not found.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] IDeleteMedicineService deleteMedicineService,
        CancellationToken cancellationToken)
    {
        var result = await deleteMedicineService.ExecuteAsync(
            new DeleteMedicineCommand(id), cancellationToken);

        if (!result.Success)
        {
            return MapServiceResult(result);
        }

        return Ok(new { message = "Medicine deleted successfully" });
    }

    /// <summary>
    /// Searches medicines by name, Arabic name, or barcode.
    /// </summary>
    /// <param name="q">The search query.</param>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="limit">Items per page (default 20).</param>
    /// <param name="searchMedicineService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Paginated search results.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromServices] ISearchMedicineService searchMedicineService = null!,
        CancellationToken cancellationToken = default)
    {
        page = page <= 0 ? 1 : page;
        limit = limit <= 0 ? 20 : limit;

        var result = await searchMedicineService.ExecuteAsync(
            new SearchMedicineQuery(q, page, limit), cancellationToken);

        if (!result.Success)
        {
            return MapServiceResult(result);
        }

        return Ok(new
        {
            medicines = result.Data!.Items,
            pagination = new
            {
                total = result.Data.Total,
                page = result.Data.Page,
                limit = result.Data.Limit
            }
        });
    }
}