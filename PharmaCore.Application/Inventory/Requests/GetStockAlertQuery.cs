namespace PharmaCore.Application.Inventory.Requests;

public record GetStockAlertQuery(int? LowStockThreshold, int? ExpiringDays, string? SearchTerm = null, int Page = 1, int Limit = 20);
