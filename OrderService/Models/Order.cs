using OrderService.Models.enums;
using OrderService.Services.DTOs;
using System.Text.Json;

namespace OrderService.Models
{
    public class Order : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string OrderNumber { get; private set; } = default!;
        public OrderStatus Status { get; private set; }
        public PaymentMethod PaymentMethod { get; private set; }
        public PaymentStatus PaymentStatus { get; private set; }
        public decimal SubTotal { get; private set; }
        public decimal DeliveryFee { get; private set; }
        public decimal Discount { get; private set; }
        public decimal Tax { get; private set; }
        public decimal Total { get; private set; }
        public Guid DeliveryAddressId { get; private set; }
        public string? DeliveryAddressJson { get; private set; }
        public Guid? CartId { get; private set; }
        public string? Notes { get; private set; }

        private readonly List<OrderItem> _items = new();
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        public Delivery? Delivery { get; private set; }
        public Payment? Payment { get; private set; }

        private Order() { }

        public static Order Create(
            Guid userId,
            string orderNumber,
            PaymentMethod paymentMethod,
            decimal subTotal,
            decimal deliveryFee,
            decimal discount,
            decimal tax,
            decimal total,
            Guid deliveryAddressId,
            string deliveryAddressJson,
            Guid? cartId = null,
            string? notes = null)
        {
            var order = new Order
            {
                UserId = userId,
                OrderNumber = orderNumber,
                Status = OrderStatus.Pending,
                PaymentMethod = paymentMethod,
                PaymentStatus = PaymentStatus.Pending,
                SubTotal = subTotal,
                DeliveryFee = deliveryFee,
                Discount = discount,
                Tax = tax,
                Total = total,
                DeliveryAddressId = deliveryAddressId,
                DeliveryAddressJson = deliveryAddressJson,
                CartId = cartId,
                Notes = notes
            };

            return order;
        }

        public void AddItem(int productId, string productName, decimal unitPrice,
                      int quantity, string? imageUrl, decimal? discount = null)  
        {
            var discountValue = discount ?? 0;  
            var item = OrderItem.Create(Id, productId, productName, unitPrice, quantity, imageUrl, discountValue);
            _items.Add(item);
        }

        public void UpdateStatus(OrderStatus status)
        {
            if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
                throw new InvalidOperationException($"Cannot update status from {Status}");

            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePaymentStatus(PaymentStatus status)
        {
            PaymentStatus = status;

            if (status == PaymentStatus.Completed && Status == OrderStatus.Pending)
            {
                Status = OrderStatus.Confirmed;
            }

            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDelivery(Delivery delivery)
        {
            Delivery = delivery;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPayment(Payment payment)
        {
            Payment = payment;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel(string reason)
        {
            if (Status == OrderStatus.Delivered)
                throw new InvalidOperationException("Cannot cancel a delivered order");

            Status = OrderStatus.Cancelled;
            Notes = $"{Notes} | Cancelled: {reason}";
            UpdatedAt = DateTime.UtcNow;
        }

        public AddressDto? GetDeliveryAddress()
        {
            if (string.IsNullOrEmpty(DeliveryAddressJson))
                return null;

            try
            {
                return JsonSerializer.Deserialize<AddressDto>(DeliveryAddressJson);
            }
            catch
            {
                return null;
            }
        }

        public static string GenerateOrderNumber()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"ORD-{timestamp}-{random}";
        }
        public void ConfirmOrder()
        {
            UpdateStatus(OrderStatus.Confirmed);
        }
    }
}

