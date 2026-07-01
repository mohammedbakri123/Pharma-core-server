using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Interfaces;

public interface ISalesReturnItemValidator
{
    Task<SalesReturnItemValidationResult> ValidateAsync(
        int salesReturnId,
        int saleItemId,
        int quantity,
        int? excludeSalesReturnItemId = null,
        CancellationToken cancellationToken = default);
}

public record SalesReturnItemValidationResult
{
    public bool IsValid => ErrorType == ServiceErrorType.None;
    public ServiceErrorType ErrorType { get; init; } = ServiceErrorType.None;
    public string? ErrorMessage { get; init; }
    public Domain.Entities.SaleItem? SaleItem { get; init; }

    public static SalesReturnItemValidationResult Ok(Domain.Entities.SaleItem saleItem) => new() { SaleItem = saleItem };
    public static SalesReturnItemValidationResult Fail(ServiceErrorType type, string message) => new() { ErrorType = type, ErrorMessage = message };
}
