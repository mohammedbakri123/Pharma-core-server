using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Payments.Services;

public static class PaymentMappings
{
    public static PaymentDto MapToDto(Payment payment) => new(
        payment.PaymentId,
        payment.Type,
        payment.ReferenceType,
        payment.ReferenceId,
        payment.Method,
        payment.UserId,
        payment.UserName,
        payment.Amount,
        payment.Description,
        payment.CreatedAt);
}
