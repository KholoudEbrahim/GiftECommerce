namespace CategoryService.Models;

public class ProductOccasion 
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int OccasionId { get; set; }
    public Occasion? Occasion { get; set; }
}
