namespace UserProfileService.Models
{
    public class DeliveryAddress : BaseEntity
    {
        public Guid UserProfileId { get; private set; }
        public string Alias { get; private set; } 
        public string Street { get; private set; }
        public string City { get; private set; }
        public string Governorate { get; private set; }
        public string Building { get; private set; }
        public string? Floor { get; private set; }
        public string? Apartment { get; private set; }
        public bool IsPrimary { get; private set; }


        // Navigation property
        public UserProfile UserProfile { get; private set; }

        // Constructor for EF
        private DeliveryAddress() { }

        public DeliveryAddress(
            Guid userProfileId,
            string alias,
            string street,
            string city,
            string governorate,
            string building,
            string? floor,
            string? apartment,
            bool isPrimary)
        {
            Id = Guid.NewGuid();
            UserProfileId = userProfileId;
            Alias = alias;
            Street = street;
            City = city;
            Governorate = governorate;
            Building = building;
            Floor = floor;
            Apartment = apartment;
            IsPrimary = isPrimary;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsPrimary()
        {
            IsPrimary = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsNotPrimary()
        {
            IsPrimary = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateAddress(
            string alias,
            string street,
            string city,
            string governorate,
            string building,
            string? floor,
            string? apartment)
        {
            Alias = alias;
            Street = street;
            City = city;
            Governorate = governorate;
            Building = building;
            Floor = floor;
            Apartment = apartment;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
