namespace IdentityService.Events
{
    public class PasswordChangedEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }
}
