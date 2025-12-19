using CartService.Models.enums;

namespace CartService.Models
{
    public class Cart : BaseEntity
    {

        public Guid? UserId { get; private set; }

        public string? AnonymousId { get; private set; }
        public CartStatus Status { get; private set; } = CartStatus.Active;
        private readonly List<CartItem> _items = new();
        public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
        public decimal SubTotal { get; private set; }
        public decimal DeliveryFee { get; private set; }
        public decimal Total => SubTotal + DeliveryFee;

        public Guid? DeliveryAddressId { get; private set; }


        private Cart() { }

        public static Cart CreateForUser(Guid userId)
        {
            return new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static Cart CreateForAnonymous(string anonymousId)
        {
            if (string.IsNullOrWhiteSpace(anonymousId))
                throw new ArgumentException("Anonymous ID is required", nameof(anonymousId));

            return new Cart
            {
                Id = Guid.NewGuid(),
                AnonymousId = anonymousId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void AddItem(CartItem item)
        {
            if (Status != CartStatus.Active)
                throw new InvalidOperationException("Cannot add items to a non-active cart");

            var existingItem = _items.FirstOrDefault(i => i.ProductId == item.ProductId);

            if (existingItem != null)
            {
                existingItem.UpdateQuantity(existingItem.Quantity + item.Quantity);
            }
            else
            {
                _items.Add(item);
            }

            RecalculateTotals();
        }

        public void UpdateItemQuantity(Guid productId, int quantity)
        {
            if (quantity <= 0)
            {
                RemoveItem(productId);
                return;
            }

            var item = _items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
                throw new KeyNotFoundException($"Item with product ID {productId} not found in cart");

            item.UpdateQuantity(quantity);
            RecalculateTotals();
        }

        public void RemoveItem(Guid productId)
        {
            var item = _items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                _items.Remove(item);
                RecalculateTotals();
            }
        }

        public void SetDeliveryAddress(Guid addressId)
        {
            DeliveryAddressId = addressId;
            DeliveryFee = CalculateDeliveryFee();
            RecalculateTotals();
        }

    

        public void Checkout()
        {
            if (Status != CartStatus.Active)
                throw new InvalidOperationException("Only active carts can be checked out");

            if (!_items.Any())
                throw new InvalidOperationException("Cannot checkout empty cart");

            Status = CartStatus.CheckedOut;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Clear()
        {
            _items.Clear();
            SubTotal = 0;
            DeliveryFee = 0;
        }

        private void RecalculateTotals()
        {
            SubTotal = _items.Sum(i => i.TotalPrice);

            
            if (DeliveryAddressId.HasValue)
            {
                DeliveryFee = CalculateDeliveryFee();
            }

            UpdatedAt = DateTime.UtcNow;
        }

        private decimal CalculateDeliveryFee()
        {
            // Default fee, in production this would come from a delivery service
            return SubTotal > 1000 ? 0 : 50; 
        }
    }
}

