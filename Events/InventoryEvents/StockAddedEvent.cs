using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.InventoryEvents
{
    public record StockAddedEvent
    {
        public int ProductId { get; init; }
        public int QuantityAdded { get; init; }
        public int NewStockLevel { get; init; }
        public DateTime AddedAt { get; init; }
        public string AddedBy { get; init; } = string.Empty; // Admin username
    }
}
