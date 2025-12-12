using CategoryService.Models.Enums;
using Shared;

namespace CategoryService.Models;

public class Occasion : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    
    public OccasionStatus Status { get; set; } = OccasionStatus.Active;
    public string? ImageUrl { get; set; }

    
    // Navigation: Link to the Join Table
    public ICollection<ProductOccasion> ProductOccasions { get; set; } = new List<ProductOccasion>();
}