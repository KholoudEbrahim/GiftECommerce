namespace UserProfileService.Features.Queries.GetProfile
{
    public class DeliveryAddressDto
    {
        public Guid Id { get; set; }
        public string Alias { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Governorate { get; set; }
        public string Building { get; set; }
        public string? Floor { get; set; }
        public string? Apartment { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
