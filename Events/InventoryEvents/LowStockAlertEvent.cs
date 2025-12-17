using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.InventoryEvents
{
    public record LowStockAlertEvent
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public int CurrentStock { get; init; }
        public int MinStock { get; init; }
        public DateTime AlertedAt { get; init; }
    }
}
