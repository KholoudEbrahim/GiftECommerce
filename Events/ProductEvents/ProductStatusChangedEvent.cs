using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.ProductEvents
{
    public record ProductStatusChangedEvent
    {
        public int ProductId { get; init; }
        public string Status { get; init; } = string.Empty; // "InStock" or "Unstock"
        public DateTime ChangedAt { get; init; }
    }
}
