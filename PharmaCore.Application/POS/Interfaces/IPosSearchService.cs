using PharmaCore.Application.POS.Dtos;
using PharmaCore.Application.POS.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.POS.Interfaces;

public interface IPosSearchService
{
    Task<ServiceResult<List<PosMedicineDto>>> ExecuteAsync(PosSearchQuery query, CancellationToken cancellationToken = default);
}
