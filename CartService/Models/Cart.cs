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
        public AddressType? DeliveryAddressType { get; private set; }
        public bool IsGift { get; private set; }
        public string? GiftMessage { get; private set; }
        public string? RecipientName { get; private set; }
        public string? RecipientPhone { get; private set; }
        public DateTime? GiftDeliveryDate { get; private set; }
        public bool GiftWrapRequested { get; private set; }
        public decimal GiftWrapFee { get; private set; } = 25;
        private Cart() { }

        public static Cart CreateForUser(Guid userId)
        {
            return new Cart
            {
                
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

        public void UpdateItemQuantity(int productId, int quantity) 
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

        public void RemoveItem(int productId) 
        {
            var item = _items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                _items.Remove(item);
                RecalculateTotals();
            }
        }

        public void SetDeliveryAddress(Guid addressId, AddressType addressType = AddressType.Home)
        {
            DeliveryAddressId = addressId;
            DeliveryAddressType = addressType;
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
           
            return SubTotal > 1000 ? 0 : 50; 
        }
        public void MarkAsGift(
             string recipientName,
               string recipientPhone,
               string? giftMessage = null,
               DateTime? deliveryDate = null,
               bool giftWrap = false)
        {
            if (string.IsNullOrWhiteSpace(recipientName))
                throw new ArgumentException("Recipient name is required", nameof(recipientName));

            if (string.IsNullOrWhiteSpace(recipientPhone))
                throw new ArgumentException("Recipient phone is required", nameof(recipientPhone));

            IsGift = true;
            RecipientName = recipientName;
            RecipientPhone = recipientPhone;
            GiftMessage = giftMessage;
            GiftDeliveryDate = deliveryDate;
            GiftWrapRequested = giftWrap;

            if (giftWrap)
            {
                GiftWrapFee = 25; 
            }

            RecalculateTotals();
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveGiftDetails()
        {
            IsGift = false;
            GiftMessage = null;
            RecipientName = null;
            RecipientPhone = null;
            GiftDeliveryDate = null;
            GiftWrapRequested = false;
            GiftWrapFee = 0;

            RecalculateTotals();
            UpdatedAt = DateTime.UtcNow;
        }

      
    }
}


