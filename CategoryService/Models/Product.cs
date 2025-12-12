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

    public int CategoryId { get; set; }
    public Category? Category { get; set; }    
    public ICollection<ProductOccasion> ProductOccasions { get; set; } = new List<ProductOccasion>();

}