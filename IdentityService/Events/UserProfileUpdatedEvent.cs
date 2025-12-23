namespace IdentityService.Events
{

    public class UserProfileUpdatedEvent
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } 
        public DateTime UpdatedAt { get; set; }
    }
}
