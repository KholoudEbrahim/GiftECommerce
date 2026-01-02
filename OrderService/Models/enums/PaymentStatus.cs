namespace OrderService.Models.enums
{
    public enum PaymentStatus
    {
        Pending = 1,
        Processing = 2,
        Completed = 3,
        Failed = 4,
        Refunded = 5,
        AwaitingCashPayment = 6,
        CashPaymentVerified = 7
    }
}
