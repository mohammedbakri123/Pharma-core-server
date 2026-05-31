using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Medicine.Requests;

public sealed record ListMedicineQuery(
    int Page = 1,
    int Limit = 20,
    string? Search = null,
    MedicineUnit? Unit = null,
    int? CategoryId = null);