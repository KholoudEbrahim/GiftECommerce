using Shared;
using System;


namespace OccasionService.Models
{
    public class Occasion : BaseEntity
    {
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;
        public string ImageUrl { get; set; }
    }
}
