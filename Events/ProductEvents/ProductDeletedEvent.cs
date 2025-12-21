using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.ProductEvents
{
    public record ProductDeletedEvent
    {
        public int ProductId { get; init; }
        public DateTime DeletedAt { get; init; }
    }
}
