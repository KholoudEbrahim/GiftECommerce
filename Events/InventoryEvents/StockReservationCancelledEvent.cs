using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.InventoryEvents
{
    public record StockReservationCancelledEvent
    {
        public int ProductId { get; init; }
        public int QuantityReleased { get; init; }
        public string OrderId { get; init; } = string.Empty;
        public DateTime CancelledAt { get; init; }
    }
}
