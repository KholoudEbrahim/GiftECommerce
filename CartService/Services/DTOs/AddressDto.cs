namespace CartService.Services.DTOs
{
    public class AddressDto
    {
        public Guid Id { get; set; }
        public string AddressLine1 { get; set; } = default!;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = default!;
        public string State { get; set; } = default!;
        public string Country { get; set; } = default!;
        public string PostalCode { get; set; } = default!;
        public bool IsDefault { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Label { get; set; }
    }

}
