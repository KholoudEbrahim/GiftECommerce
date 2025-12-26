using OrderService.Models.enums;

namespace OrderService.Models
{
    public class Delivery : BaseEntity
    {
        public int OrderId { get; private set; }
        public DeliveryStatus Status { get; private set; }
        public DateTime? EstimatedDeliveryTime { get; private set; }
        public DateTime? ActualDeliveryTime { get; private set; }
        public string? DeliveryHeroId { get; private set; }
        public string? DeliveryHeroName { get; private set; }
        public string? DeliveryHeroPhone { get; private set; }
        public decimal? CurrentLatitude { get; private set; }
        public decimal? CurrentLongitude { get; private set; }
        public string? TrackingUrl { get; private set; }
        public string? Notes { get; private set; }

        // Navigation
        public Order Order { get; private set; } = default!;

        private Delivery() { }

        public static Delivery Create(int orderId)
        {
            return new Delivery
            {
                OrderId = orderId,
                Status = DeliveryStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void AssignHero(string heroId, string heroName, string heroPhone)
        {
            if (string.IsNullOrWhiteSpace(heroId))
                throw new ArgumentException("Hero ID is required", nameof(heroId));

            DeliveryHeroId = heroId;
            DeliveryHeroName = heroName;
            DeliveryHeroPhone = heroPhone;
            Status = DeliveryStatus.Assigned;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateLocation(decimal latitude, decimal longitude)
        {
            CurrentLatitude = latitude;
            CurrentLongitude = longitude;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(DeliveryStatus status, string? notes = null)
        {
            Status = status;
            Notes = notes;

            if (status == DeliveryStatus.Delivered)
            {
                ActualDeliveryTime = DateTime.UtcNow;
            }

            UpdatedAt = DateTime.UtcNow;
        }

        public void SetEstimatedDelivery(DateTime estimatedTime)
        {
            EstimatedDeliveryTime = estimatedTime;
            UpdatedAt = DateTime.UtcNow;
        }

        public void GenerateTrackingUrl()
        {
            TrackingUrl = $"https://maps.google.com/?q={CurrentLatitude},{CurrentLongitude}";
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
