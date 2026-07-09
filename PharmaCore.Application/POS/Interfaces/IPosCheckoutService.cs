using PharmaCore.Application.POS.Dtos;
using PharmaCore.Application.POS.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.POS.Interfaces;

public interface IPosCheckoutService
{
    Task<ServiceResult<PosCheckoutResultDto>> ExecuteAsync(PosCheckoutCommand command, CancellationToken cancellationToken = default);
}
