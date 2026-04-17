using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.Payments;
using PharmaCore.Application.Payments.Interfaces;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Domain.Enums;

namespace PharmaCore.API.Controllers;

/// <summary>
/// Manages incoming and outgoing payments.
/// </summary>
[Route("payments")]
[Authorize]
[Tags("Payments")]
public class PaymentsController : ApiControllerBase
{
    /// <summary>
    /// Creates a payment.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePaymentRequest request,
        [FromServices] ICreatePaymentService createPaymentService,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : null;

        var result = await createPaymentService.ExecuteAsync(
            new CreatePaymentCommand(
                request.Type,
                request.ReferenceType,
                request.ReferenceId,
                request.Method,
                request.Amount,
                request.Description,
                userId),
            cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// Returns a paginated list of payments.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] PaymentType? type = null,
        [FromQuery] PaymentMethod? method = null,
        [FromQuery] PaymentReferenceType? referenceType = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromServices] IListPaymentsService listPaymentsService = null!,
        CancellationToken cancellationToken = default)
    {
        var result = await listPaymentsService.ExecuteAsync(
            new ListPaymentsQuery(page, limit, type, method, referenceType, from, to),
            cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new
        {
            payments = result.Data!.Items,
            pagination = new
            {
                total = result.Data.Total,
                page = result.Data.Page,
                limit = result.Data.Limit
            }
        });
    }

    /// <summary>
    /// Returns a payment by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] IGetPaymentByIdService getPaymentByIdService,
        CancellationToken cancellationToken)
    {
        var result = await getPaymentByIdService.ExecuteAsync(id, cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Returns payments for a sale.
    /// </summary>
    [HttpGet("sale/{saleId:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSalePayments(
        int saleId,
        [FromServices] IGetPaymentsByReferenceService getPaymentsByReferenceService,
        CancellationToken cancellationToken)
    {
        var result = await getPaymentsByReferenceService.ExecuteAsync(PaymentReferenceType.SALE, saleId, cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new
        {
            saleId,
            payments = result.Data!.Payments,
            totalPaid = result.Data.TotalPaid
        });
    }

    /// <summary>
    /// Returns payments for a purchase.
    /// </summary>
    [HttpGet("purchase/{purchaseId:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPurchasePayments(
        int purchaseId,
        [FromServices] IGetPaymentsByReferenceService getPaymentsByReferenceService,
        CancellationToken cancellationToken)
    {
        var result = await getPaymentsByReferenceService.ExecuteAsync(PaymentReferenceType.PURCHASE, purchaseId, cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new
        {
            purchaseId,
            payments = result.Data!.Payments,
            totalPaid = result.Data.TotalPaid
        });
    }
}
