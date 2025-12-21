using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.InventoryEvents
{
    public record StockUpdatedEvent
    {
        public int ProductId { get; init; }
        public int CurrentStock { get; init; }
        public int MinStock { get; init; }
        public int MaxStock { get; init; }
        public DateTime UpdatedAt { get; init; }
        public bool IsLowStock { get; init; } // true if CurrentStock <= MinStock
    }
}
