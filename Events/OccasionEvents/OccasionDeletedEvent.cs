using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.OccasionEvents
{
    public record OccasionDeletedEvent
    {
        public Guid OccasionId { get; init; }
        public DateTime DeletedAt { get; init; }
    }
}
