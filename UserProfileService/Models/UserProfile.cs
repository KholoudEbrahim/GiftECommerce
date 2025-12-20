namespace UserProfileService.Models
{
    public class UserProfile : BaseEntity
    {

        public Guid UserId { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string? PhoneNumber { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public string? ProfilePictureUrl { get; private set; }


        // Navigation property
        private readonly List<DeliveryAddress> _deliveryAddresses = new();
        public IReadOnlyCollection<DeliveryAddress> DeliveryAddresses => _deliveryAddresses.AsReadOnly();

        // Constructor for EF
        private UserProfile() { }

        public UserProfile(Guid userId, string firstName, string lastName)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Domain methods
        public void UpdateProfile(string firstName, string lastName, string? phoneNumber, DateTime? dateOfBirth)
        {
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            DateOfBirth = dateOfBirth;
            UpdatedAt = DateTime.UtcNow;
        }

        public DeliveryAddress AddDeliveryAddress(
        string alias,
        string street,
        string city,
        string governorate,
        string building,
        string? floor,
        string? apartment,
        bool isPrimary)
        {
            if (isPrimary)
            {
                foreach (var existingAddress in _deliveryAddresses) 
                {
                    existingAddress.MarkAsNotPrimary(); 
                }
            }

            var newAddress = new DeliveryAddress( 
                Id,
                alias,
                street,
                city,
                governorate,
                building,
                floor,
                apartment,
                isPrimary);

            _deliveryAddresses.Add(newAddress); // ✅ غيري هنا
            return newAddress; // ✅ غيري هنا
        }
    }
}