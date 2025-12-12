using CategoryService.Models.Enums;
using Shared;
namespace CategoryService.Models;


public class Category : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public CategoryStatus Status { get; set; } = CategoryStatus.Active; 
    public string? ImageUrl { get; set; }
    

    // Navigation: A category contains many products
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
