using OfferService.Models.enums;
using Shared;

namespace OfferService.Models;

public class Offer : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Is it 10% off or $50 off?
    public DiscountType Type { get; set; } = DiscountType.Percentage;
    public decimal Value { get; set; } // The amount (e.g. 10.0 or 50.0)

    // Validity Period
    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }
    public bool IsActive { get; set; } = true;

    // TARGETING: An offer usually applies to ONE of these.
    // If all are null, maybe it's a global site-wide sale.
    public int? ProductId { get; set; }
    public int? CategoryId { get; set; }
    public int? OccasionId { get; set; }

    // Logic helper: Is this offer currently valid?
    public bool IsValid()
    {
        var now = DateTime.UtcNow;
        return IsActive && !IsDeleted && now >= StartDateUtc && now <= EndDateUtc;
    }
}
