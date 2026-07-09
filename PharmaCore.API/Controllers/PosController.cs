using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.POS;
using PharmaCore.Application.POS.Interfaces;
using PharmaCore.Application.POS.Requests;

namespace PharmaCore.API.Controllers;

[Route("pos")]
[Authorize]
[Tags("POS")]
public class PosController : ApiControllerBase
{
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Checkout(
        [FromBody] PosCheckoutRequest request,
        [FromServices] IPosCheckoutService posCheckoutService,
        CancellationToken cancellationToken)
    {
        var command = new PosCheckoutCommand(
            TryGetUserId(),
            request.CustomerId,
            request.Discount,
            request.Note,
            request.Payment.Method,
            request.Payment.Amount,
            request.Items.Select(i => new PosCheckoutItem(i.MedicineId, i.Quantity)).ToList());

        var result = await posCheckoutService.ExecuteAsync(command, cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return Created("/pos/checkout", result.Data);
    }
}
