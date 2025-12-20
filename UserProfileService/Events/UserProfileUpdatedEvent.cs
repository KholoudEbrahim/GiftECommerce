namespace UserProfileService.Events
{
    public class UserProfileUpdatedEvent
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
