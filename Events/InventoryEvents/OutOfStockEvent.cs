using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.InventoryEvents
{
    public record OutOfStockEvent
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public DateTime OutOfStockAt { get; init; }
    }
}
