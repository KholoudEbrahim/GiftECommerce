using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.ProductEvents
{
    public record ProductCreatedEvent
    {
        public int ProductId { get; init; }
        public string Name { get; init; } = string.Empty;
        public int CategoryId { get; init; }
        public string CategoryName { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
