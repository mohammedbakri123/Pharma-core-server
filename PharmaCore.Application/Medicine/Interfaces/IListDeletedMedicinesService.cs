using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Interfaces;

public interface IListDeletedMedicinesService
{
    Task<ServiceResult<PagedResult<MedicineDto>>> ExecuteAsync(ListDeletedMedicinesQuery query, CancellationToken cancellationToken = default);
}
