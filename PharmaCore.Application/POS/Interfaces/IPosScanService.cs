using PharmaCore.Application.POS.Dtos;
using PharmaCore.Application.POS.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.POS.Interfaces;

public interface IPosScanService
{
    Task<ServiceResult<PosMedicineDto>> ExecuteAsync(PosScanQuery query, CancellationToken cancellationToken = default);
}
