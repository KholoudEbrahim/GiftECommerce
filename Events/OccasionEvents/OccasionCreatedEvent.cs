using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.OccasionEvents
{
    public record OccasionCreatedEvent
    {
        public Guid OccasionId { get; init; }
        public string Name { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
