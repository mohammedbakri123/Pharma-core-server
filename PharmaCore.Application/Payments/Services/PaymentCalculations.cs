namespace PharmaCore.Application.Payments.Services;

public static class PaymentCalculations
{
    public static decimal ComputeSaleRemaining(decimal totalAmount, decimal paidAmount, decimal discount, decimal returnedAmount)
        => totalAmount - paidAmount - discount - returnedAmount;

    public static decimal ComputePurchaseRemaining(decimal totalAmount, decimal paidAmount, decimal returnedAmount)
        => totalAmount - paidAmount - returnedAmount;

    public static decimal ComputePurchaseReturnMaxRefund(decimal purchaseTotalAmount, decimal purchaseReturnTotalAmount, decimal totalPaidOnPurchase)
    {
        var goodsKept = Math.Max(0, purchaseTotalAmount - purchaseReturnTotalAmount);
        var overpaid = totalPaidOnPurchase - goodsKept;
        return Math.Max(0, overpaid);
    }

    public static decimal ComputeSalesReturnMaxRefund(decimal saleTotalAmount, decimal salesReturnTotalAmount, decimal totalPaidOnSale)
    {
        var goodsKept = Math.Max(0, saleTotalAmount - salesReturnTotalAmount);
        var overpaid = totalPaidOnSale - goodsKept;
        return Math.Max(0, overpaid);
    }
}
