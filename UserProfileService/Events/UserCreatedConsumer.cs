using MassTransit;
using UserProfileService.Data;
using UserProfileService.Models;

namespace UserProfileService.Events
{
    public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly IUserProfileRepository _repository;
        private readonly ILogger<UserCreatedConsumer> _logger;

        public UserCreatedConsumer(
            IUserProfileRepository repository,
            ILogger<UserCreatedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            try
            {
                var @event = context.Message;

                _logger.LogInformation(
                    " Received UserCreatedEvent for user: {UserId} ({Email})",
                    @event.UserId, @event.Email);

                var existingProfile = await _repository.GetByUserIdAsync(@event.UserId);

                if (existingProfile != null)
                {
                    _logger.LogWarning(
                        " Profile already exists for user: {UserId}. Skipping...",
                        @event.UserId);
                    return;
                }

    
                var profile = new UserProfile(
                    @event.UserId,
                    @event.FirstName ?? "User",
                    @event.LastName ?? "User");

     
                if (!string.IsNullOrEmpty(@event.PhoneNumber))
                {
                    profile.UpdateProfile(
                        @event.FirstName ?? "User",
                        @event.LastName ?? "User",
                        @event.PhoneNumber,
                        null);
                }

         
                await _repository.AddAsync(profile);
                await _repository.SaveChangesAsync();

                _logger.LogInformation(
                    " Profile created successfully for user: {UserId}. Profile ID: {ProfileId}",
                    @event.UserId, profile.Id);

         
                await context.Publish(new ProfileCreatedEvent
                {
                    UserId = @event.UserId,
                    ProfileId = profile.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error processing UserCreatedEvent for user {UserId}",
                    context.Message?.UserId);


                throw;
            }
        }
    }


    public class ProfileCreatedEvent
    {
        public Guid UserId { get; set; }
        public Guid ProfileId { get; set; }
        public DateTime CreatedAt { get; set; }
    }




    public class UserCreatedEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
