using Shared;

namespace InventoryService.Models
{
    public class Stock : BaseEntity<int>
    {
        public int ProductId { get; set; } // Foreign Key from CategoryService
        public string ProductName { get; set; } = string.Empty;

        public int CurrentStock { get; set; } = 0;
        public int MinStock { get; set; } = 0;
        public int MaxStock { get; set; } = 100;

        public bool IsLowStock => CurrentStock <= MinStock;
        public bool IsOutOfStock => CurrentStock == 0;

        // Navigation Property
        public ICollection<StockTransaction> Transactions { get; set; } = new List<StockTransaction>();
    }
}
