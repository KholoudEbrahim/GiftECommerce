namespace UserProfileService.Features.Commands.DeliveryAddress
{
    public class AddDeliveryAddressRequest
    {
        public string Alias { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Governorate { get; set; }
        public string Building { get; set; }
        public string? Floor { get; set; }
        public string? Apartment { get; set; }
        public bool IsPrimary { get; set; }
    }
}
