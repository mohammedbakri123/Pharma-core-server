namespace PharmaCore.Application.Inventory.Requests;

public record GetStockAlertQuery(int? LowStockThreshold, int? ExpiringDays);
