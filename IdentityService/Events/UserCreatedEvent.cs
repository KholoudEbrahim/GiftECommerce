namespace IdentityService.Events
{
    public class UserCreatedEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
