using System.ComponentModel.DataAnnotations;

namespace UserProfileService.Models
{
    public abstract class BaseEntity<TKey>
    {
        public TKey Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        [Timestamp]
        public byte[] RowVersion { get; set; }

    }
    public abstract class BaseEntity : BaseEntity<Guid>
    {
    }
}
