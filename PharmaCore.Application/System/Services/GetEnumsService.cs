using System.Text.Json;
using PharmaCore.Application.System.Dtos;
using PharmaCore.Application.System.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.System.Services;

public class GetEnumsService : IGetEnumsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Task<ServiceResult<EnumsDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var enums = new Dictionary<string, IReadOnlyList<EnumValueDto>>
        {
            ["medicineUnit"] = ToDto<MedicineUnit>(),
            ["paymentMethod"] = ToDto<PaymentMethod>(),
            ["paymentReferenceType"] = ToDto<PaymentReferenceType>(),
            ["paymentType"] = ToDto<PaymentType>(),
            ["purchaseStatus"] = ToDto<PurchaseStatus>(),
            ["saleStatus"] = ToDto<SaleStatus>(),
            ["stockMovementReferenceType"] = ToDto<StockMovementReferenceType>(),
            ["stockMovementType"] = ToDto<StockMovementType>(),
            ["userRole"] = ToDto<UserRole>()
        };

        return Task.FromResult(ServiceResult<EnumsDto>.Ok(new EnumsDto(enums)));
    }

    private static IReadOnlyList<EnumValueDto> ToDto<TEnum>() where TEnum : struct, Enum
    {
        var namingPolicy = JsonNamingPolicy.CamelCase;
        return Enum.GetValues<TEnum>()
            .Select(value => new EnumValueDto(
                namingPolicy.ConvertName(value.ToString()),
                (short)(object)value))
            .ToList();
    }
}
