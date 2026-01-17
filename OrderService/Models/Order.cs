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
        public int? CartId { get; private set; }
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
            int? cartId = null,
            string? notes = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(orderNumber))
                throw new ArgumentException("Order number is required", nameof(orderNumber));

            if (deliveryAddressId == Guid.Empty)
                throw new ArgumentException("Delivery address ID cannot be empty", nameof(deliveryAddressId));

            if (string.IsNullOrWhiteSpace(deliveryAddressJson))
                throw new ArgumentException("Delivery address is required", nameof(deliveryAddressJson));

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

            if (Id == 0)
                throw new InvalidOperationException("Cannot add items to an unsaved order");

            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException($"Cannot add items to order with status {Status}");


            var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            }
            else
            {
                var discountValue = discount ?? 0;
                var item = OrderItem.Create(Id, productId, productName, unitPrice, quantity, imageUrl, discountValue);
                _items.Add(item);
            }

            RecalculateTotals();
        }

        public void RemoveItem(int orderItemId)
        {
            var item = _items.FirstOrDefault(i => i.Id == orderItemId);
            if (item == null)
                throw new KeyNotFoundException($"Order item {orderItemId} not found");

            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException($"Cannot remove items from order with status {Status}");

            _items.Remove(item);
            RecalculateTotals();
        }
        private void RecalculateTotals()
        {
            SubTotal = _items.Sum(i => i.TotalPrice);
            Tax = SubTotal * 0.14m;

            Total = SubTotal + Tax + DeliveryFee - Discount;

            UpdatedAt = DateTime.UtcNow;
        }


        public void UpdateStatus(OrderStatus status)
        {
            if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
                throw new InvalidOperationException($"Cannot update status from {Status}");

            ValidateStatusTransition(status);

            var oldStatus = Status;
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }
        private void ValidateStatusTransition(OrderStatus newStatus)
        {
            var allowedTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
            {
                { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.Confirmed, OrderStatus.Cancelled, OrderStatus.Failed } },
                { OrderStatus.Confirmed, new List<OrderStatus> { OrderStatus.Processing, OrderStatus.Cancelled } },
                { OrderStatus.Processing, new List<OrderStatus> { OrderStatus.ReadyForDelivery, OrderStatus.Cancelled } },
                { OrderStatus.ReadyForDelivery, new List<OrderStatus> { OrderStatus.OutForDelivery, OrderStatus.Cancelled } },
                { OrderStatus.OutForDelivery, new List<OrderStatus> { OrderStatus.Delivered, OrderStatus.Failed } },
                { OrderStatus.Delivered, new List<OrderStatus>() },
                { OrderStatus.Cancelled, new List<OrderStatus>() },
                { OrderStatus.Failed, new List<OrderStatus> { OrderStatus.Pending } }
            };

            if (!allowedTransitions[Status].Contains(newStatus))
            {
                throw new InvalidOperationException(
                    $"Cannot transition order status from {Status} to {newStatus}");
            }
        }

        public void UpdatePaymentStatus(PaymentStatus status)
        {
            PaymentStatus = status;

            if (status == PaymentStatus.Completed && Status == OrderStatus.Pending)
            {
                Status = OrderStatus.Confirmed;
            }
            else if (status == PaymentStatus.Failed && Status == OrderStatus.Pending)
            {
                Status = OrderStatus.Failed;
            }

            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDelivery(Delivery delivery)
        {
            Delivery = delivery ?? throw new ArgumentNullException(nameof(delivery));
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPayment(Payment payment)
        {
            Payment = payment ?? throw new ArgumentNullException(nameof(payment));
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel(string reason)
        {
            if (Status == OrderStatus.Delivered)
                throw new InvalidOperationException("Cannot cancel a delivered order");

            if (Status == OrderStatus.OutForDelivery)
                throw new InvalidOperationException("Cannot cancel an order that is out for delivery. Please contact support.");

            Status = OrderStatus.Cancelled;
            Notes = string.IsNullOrEmpty(Notes)
                ? $"Cancelled: {reason}"
                : $"{Notes} | Cancelled: {reason}";
            UpdatedAt = DateTime.UtcNow;
        }

        public void ConfirmOrder()
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException($"Cannot confirm order with status {Status}");

            UpdateStatus(OrderStatus.Confirmed);
        }

        public AddressDto? GetDeliveryAddress()
        {
            if (string.IsNullOrEmpty(DeliveryAddressJson))
                return null;

            try
            {
                return JsonSerializer.Deserialize<AddressDto>(DeliveryAddressJson);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GenerateOrderNumber()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var uniqueId = Guid.NewGuid().ToString("N")[..8].ToUpper();
            return $"ORD-{timestamp}-{uniqueId}";
        }


        public bool CanBeCancelled() =>
            Status != OrderStatus.Delivered &&
            Status != OrderStatus.Cancelled &&
            Status != OrderStatus.OutForDelivery;

        public bool CanBeModified() =>
            Status == OrderStatus.Pending;

        public bool IsInProgress() =>
            Status != OrderStatus.Delivered &&
            Status != OrderStatus.Cancelled &&
            Status != OrderStatus.Failed;
    
        public void AddNote(string note)
        {
            if (string.IsNullOrWhiteSpace(note))
                return;

            Notes = string.IsNullOrEmpty(Notes)
                ? note
                : $"{Notes} | {note}";

            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsRefunded(string reason)
        {
            AddNote($"Refunded: {reason}");
        }

    }
}

