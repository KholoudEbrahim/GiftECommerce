using CategoryService.Models.Enums;
using Shared;
namespace CategoryService.Models;


public class Product : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? Discount { get; set; } // Nullable for no discount
    public ProductStatus Status { get; set; } = ProductStatus.InStock; // InStock, OutOfStock
    public string? ImageUrl { get; set; }

    public string? Tags { get; set; } // Stored as JSON or comma-separated

    // NEW: Best Sellers tracking
    public int TotalSales { get; set; } = 0;
    public int ViewCount { get; set; } = 0;
    public decimal Rating { get; set; } = 0;
    public int RatingCount { get; set; } = 0;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }    
    public ICollection<ProductOccasion> ProductOccasions { get; set; } = new List<ProductOccasion>();

    public List<string> TagsList
    {
        get => string.IsNullOrWhiteSpace(Tags)
            ? new List<string>()
            : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(t => t.Trim())
                  .ToList();
        set => Tags = value != null && value.Any()
            ? string.Join(",", value.Select(t => t.Trim()))
            : null;
    }

    public decimal PopularityScore => (TotalSales * 10) + (ViewCount * 0.1m);

}