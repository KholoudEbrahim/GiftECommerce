using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.OccasionEvents
{
    public record OccasionStatusChangedEvent
    {
        public Guid OccasionId { get; init; }
        public bool IsActive { get; init; }
        public DateTime ChangedAt { get; init; }
    }
}
