using InventoryService.Models.Enums;
using Shared;

namespace InventoryService.Models
{
    public class StockTransaction : BaseEntity<int>
    {
        public int StockId { get; set; }
        public Stock? Stock { get; set; }

        public StockTransactionType Type { get; set; }

        public int Quantity { get; set; }
        public int StockBefore { get; set; }
        public int StockAfter { get; set; }

        public string? Reference { get; set; }
        public string? Notes { get; set; } 
        public string? PerformedBy { get; set; }       // (Admin username)
    }
}
