using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Interfaces;

public interface IPurchaseReturnItemValidator
{
    Task<PurchaseReturnItemValidationResult> ValidateAsync(
        int purchaseReturnId,
        int purchaseItemId,
        int quantity,
        int? excludePurchaseReturnItemId = null,
        CancellationToken cancellationToken = default);
}

public record PurchaseReturnItemValidationResult
{
    public bool IsValid => ErrorType == ServiceErrorType.None;
    public ServiceErrorType ErrorType { get; init; } = ServiceErrorType.None;
    public string? ErrorMessage { get; init; }
    public Domain.Entities.PurchaseItem? PurchaseItem { get; init; }

    public static PurchaseReturnItemValidationResult Ok(Domain.Entities.PurchaseItem purchaseItem) => new() { PurchaseItem = purchaseItem };
    public static PurchaseReturnItemValidationResult Fail(ServiceErrorType type, string message) => new() { ErrorType = type, ErrorMessage = message };
}
