using System.Text.Json;

namespace OrderService.Services.DTOs
{
    public class AddressDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string Street { get; set; } = default!;
        public string City { get; set; } = default!;
        public string State { get; set; } = default!;
        public string Country { get; set; } = default!;
        public string PostalCode { get; set; } = default!;
        public bool IsDefault { get; set; }
        public string? Landmark { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public override string ToString()
        {
            return $"{Street}, {City}, {State}, {Country}, {PostalCode}";
        }
    }
}
