using MassTransit;
using System.Text.Json;
using UserProfileService.Data;
using UserProfileService.Models;
using Microsoft.EntityFrameworkCore;
using IdentityService.Events;
namespace UserProfileService.Events
{
    public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly IUserProfileRepository _repository;
        private readonly UserProfileDbContext _dbContext;
        private readonly ILogger<UserCreatedConsumer> _logger;

        public UserCreatedConsumer(
            IUserProfileRepository repository,
            UserProfileDbContext dbContext,
            ILogger<UserCreatedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
  
            _logger.LogInformation("=== EVENT RECEIVED === {Event}", JsonSerializer.Serialize(context.Message));

            try
            {
                var @event = context.Message;

  
                var canConnect = await _dbContext.Database.CanConnectAsync();
                _logger.LogInformation($"Database can connect: {canConnect}");

                var existingProfile = await _repository.GetByUserIdAsync(@event.UserId);
                if (existingProfile != null)
                {
                    _logger.LogWarning(
                        "Profile already exists for user: {UserId}. Skipping...",
                        @event.UserId);
                    return;
                }

  
                var profile = new UserProfile(
                    @event.UserId,
                    @event.FirstName ?? "User",
                    @event.LastName ?? "User");

                if (!string.IsNullOrEmpty(@event.Phone))
                {
                    profile.UpdateProfile(
                        @event.FirstName ?? "User",
                        @event.LastName ?? "User",
                        @event.Phone,
                        null);
                }

                await _repository.AddAsync(profile);
                await _repository.SaveChangesAsync();

                _logger.LogInformation(
                    "✓ Profile created successfully for user: {UserId}. Profile ID: {ProfileId}",
                    @event.UserId, profile.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "✗ Error processing UserCreatedEvent for user {UserId}",
                    context.Message?.UserId);
                throw;
            }
        }
    }

}



public class ProfileCreatedEvent
    {
        public Guid UserId { get; set; }
        public Guid ProfileId { get; set; }
        public DateTime CreatedAt { get; set; }
    }





