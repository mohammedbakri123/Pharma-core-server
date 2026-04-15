namespace PharmaCore.Application.Medicine.Requests;

public sealed record SearchMedicineQuery(string Q, int Page = 1, int Limit = 20);
